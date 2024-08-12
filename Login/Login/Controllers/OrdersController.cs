using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Login.Data;
using System.Globalization;
using PagedList.Core;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Pqc.Crypto.Lms;
namespace Login.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context; // Dependency Injection for the database context
        private readonly ILogger<OrdersController> _logger; // Dependency Injection for logging

        // Constructor to inject dependencies
        public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Orders/UserOrders/5
        [Route("Orders/UserOrders/{userId}")]
        public async Task<IActionResult> UserOrders(int? userId, int? page, string transactionType, string isSip, string startDate, string endDate)
        {
            if (userId == null)
            {
                return NotFound(); // Returns 404 if the userId is null
            }

            int pageNumber = page ?? 1; // Default to page 1 if no page is specified
            int pageSize = 4; // Number of items per page

            try
            {
                // Query to fetch orders with related entities
                var orders = _context.Orders
                    .Include(o => o.MutualFund)
                    .Include(o => o.UserDetails)
                    .Include(o => o.UserFund)
                    .Where(o => o.UserId == userId) // Filter orders by userId
                    .AsQueryable();

                // Apply filters based on the query parameters
                if (!string.IsNullOrEmpty(transactionType))
                {
                    orders = orders.Where(o => o.TransactionType == transactionType);
                }

                if (!string.IsNullOrEmpty(isSip))
                {
                    bool isSipValue = bool.Parse(isSip);
                    orders = orders.Where(o => o.IsSip == isSipValue);
                }

                if (!string.IsNullOrEmpty(startDate))
                {
                    DateTime startDateValue = DateTime.Parse(startDate);
                    orders = orders.Where(o => o.Timestamp >= startDateValue);
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    DateTime endDateValue = DateTime.Parse(endDate);
                    orders = orders.Where(o => o.Timestamp <= endDateValue);
                }
                _logger.LogInformation("Orders fetched successfully."); // Logging information

                var
 pagedOrders = orders.OrderByDescending(o => o.Timestamp).ToPagedList(pageNumber, pageSize); // Paginating the orders

                return View(pagedOrders); // Returning the paginated orders to the view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user orders"); // Logging error
                return StatusCode(500, "Internal server error"); // Returning 500 internal server error
            }
        }


        [Route("Orders/DownloadUserOrdersPdf")]
        public async Task<IActionResult> DownloadUserOrdersPdf(int? userId, string transactionType, string isSip, string startDate, string endDate)
        {
            if (userId == null)
            {
                return NotFound(); // Returns 404 if the userId is null
            }

            try
            {
                var query = _context.Orders
                    .Include(o => o.MutualFund)
                    .Include(o => o.UserDetails)
                    .Include(o => o.UserFund)
                    .Where(o => o.UserId == userId) // Filter orders by userId
                    .AsQueryable();

                // Apply filters based on the query parameters
                if (!string.IsNullOrEmpty(transactionType))
                {
                    query = query.Where(o => o.TransactionType == transactionType);
                }

                if (!string.IsNullOrEmpty(isSip))
                {
                    bool isSipBool = bool.Parse(isSip);
                    query = query.Where(o => o.IsSip == isSipBool);
                }

                if (!string.IsNullOrEmpty(startDate))
                {
                    DateTime start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
                    query = query.Where(o => o.Timestamp >= start);
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    DateTime end = DateTime.Parse(endDate, CultureInfo.InvariantCulture);
                    query = query.Where(o => o.Timestamp <= end);
                }

                var
 orders =
 await
 query.OrderByDescending(o => o.Timestamp).ToListAsync();

                using (var memoryStream = new MemoryStream())
                {
                    var writer = new PdfWriter(memoryStream);
                    var pdf = new PdfDocument(writer);
                    var document = new Document(pdf);

                    // Add the title with green color
                    document.Add(new Paragraph("Investment Portfolio Management")
                        .SetBold()
                        .SetFontSize(20)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetMarginBottom(20));

                    // Add User Details at the top
                    if (orders.Any())
                    {
                        var firstOrder = orders.First();
                        var userName = firstOrder.UserDetails.Name;
                        var userDematId = firstOrder.UserDetails.DematId;
                        var userEmail = firstOrder.UserDetails.Email;

                        document.Add(new Paragraph($"Name: {userName}")
                            .SetBold()
                            .SetFontSize(14));
                        document.Add(new Paragraph($"Demat ID: {userDematId}")
                            .SetFontSize(12));
                        document.Add(new Paragraph($"Email: {userEmail}")
                            .SetFontSize(12));
                        document.Add(new Paragraph("\n"));
                    }

                    // Add styled table
                    var table = new Table(8)
                        .UseAllAvailableWidth()
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER)
                        .SetMarginBottom(20);

                    // Table header styling
                    var headerCellStyle = new Style()
                        .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                        .SetBold()
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetPadding(5)
                        .SetBorder(new iText.Layout.Borders.SolidBorder(iText.Kernel.Colors.ColorConstants.BLACK, 1));

                    table.AddHeaderCell(new Cell().Add(new Paragraph("Order ID")).AddStyle(headerCellStyle));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Fund Name")).AddStyle(headerCellStyle));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Units")).AddStyle(headerCellStyle));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Unit Price")).AddStyle(headerCellStyle));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Amount")).AddStyle(headerCellStyle));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Is SIP")).AddStyle(headerCellStyle));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Timestamp")).AddStyle(headerCellStyle));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Transaction Type")).AddStyle(headerCellStyle));

                    // Table row styling
                    var cellStyle = new Style()
                        .SetPadding(5)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetBorder(new iText.Layout.Borders.SolidBorder(iText.Kernel.Colors.ColorConstants.BLACK, 1));

                    foreach (var order in orders)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(order.OrderId.ToString())).AddStyle(cellStyle));
                        table.AddCell(new Cell().Add(new Paragraph(order.MutualFund.FundName)).AddStyle(cellStyle));
                        table.AddCell(new Cell().Add(new Paragraph(order.Units.ToString())).AddStyle(cellStyle));
                        table.AddCell(new Cell().Add(new Paragraph(order.UnitPrice.ToString())).AddStyle(cellStyle));
                        table.AddCell(new Cell().Add(new Paragraph(order.Amount.ToString())).AddStyle(cellStyle));
                        table.AddCell(new Cell().Add(new Paragraph(order.IsSip.ToString())).AddStyle(cellStyle));
                        table.AddCell(new Cell().Add(new Paragraph(order.Timestamp.ToString())).AddStyle(cellStyle));
                        table.AddCell(new Cell().Add(new Paragraph(order.TransactionType)).AddStyle(cellStyle));
                    }

                    document.Add(table);
                    document.Close();

                    return File(memoryStream.ToArray(), "application/pdf", "user_orders.pdf");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the PDF"); // Logging error
                return StatusCode(500, "Internal server error"); // Returning 500 internal server error
            }
        }


        // GET: Orders/Details/5
        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Returns 404 if the ID is null
            }

            try
            {
                // Fetching the order details from the database including related MutualFund entity
                var order = await _context.Orders
                    .Include(o => o.MutualFund)
                    .FirstOrDefaultAsync(m => m.OrderId == id);

                if (order == null)
                {
                    return NotFound(); // Returns 404 if the order is not found
                }
                _logger.LogInformation("Orders details fetched successfully."); // Logging information
                return View(order); // Returning the order details to the view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the order details for Order ID {OrderID}", id); // Logging error
                return StatusCode(500, "Internal server error"); // Returning 500 internal server error
            }
        }

    }
}
