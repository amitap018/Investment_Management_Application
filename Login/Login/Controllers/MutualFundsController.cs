
using Microsoft.AspNetCore.Mvc;
using Login.Data;
using Login.Models;
using Login.ViewModels;
using System.Security.Claims;
using Login.Services;
using Microsoft.AspNetCore.Authorization;


namespace Investment2.Controllers
{


    // Define the route prefix for all actions in this controller
    [Authorize]
    [Route("mutual-funds")]
    public class MutualFundsController : Controller
    {
        // Dependency Injection: Inject ApplicationDbContext and ILogger for database access and logging
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MutualFundsController> _logger;
        private readonly IEmailService _emailService;

        // Constructor for initializing the controller with dependency-injected services
        public MutualFundsController(ApplicationDbContext context, ILogger<MutualFundsController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        // GET: mutual-funds (Route: /mutual-funds or /mutual-funds/index)
        [HttpGet]
        [Route("")]
        [Route("index")]
        public IActionResult Index()
        {
            try
            {
                // Retrieve a list of mutual funds from the database
                var mutualFunds = _context.MutualFunds.ToList();
                _logger.LogInformation("Retrieved list of mutual funds.");
                return View(mutualFunds); // Pass the data to the view for rendering
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the list of mutual funds.");
                return StatusCode(500, "Internal server error"); // Return a 500 error response in case of failure
            }
        }

        // GET: mutual-funds/performance (Route: /mutual-funds/performance)
        [HttpGet]
        [Route("performance")]
        public IActionResult GetFundPerformance(string fund)
        {
            List<int> performanceData;
            // Use a switch statement to select performance data based on the fund type
            switch (fund)
            {
                case "Balanced Fund":
                    performanceData = RandomIntegerData.BalancedFundPerformance;
                    break;
                case "Global Debt Fund":
                    performanceData = RandomIntegerData.GlobalDebtFundPerformance;
                    break;
                case "Global Equity Fund":
                    performanceData = RandomIntegerData.GlobalEquityFundPerformance;
                    break;
                case "High-Cap Balanced Fund":
                    performanceData = RandomIntegerData.HighCapBalancedFundPerformance;
                    break;
                case "High-Cap Debt Fund":
                    performanceData = RandomIntegerData.HighCapDebtFundPerformance;
                    break;
                case "Low-Cap Debt Fund":
                    performanceData = RandomIntegerData.LowCapDebtFundPerformance;
                    break;
                case "Low-Cap Equity Fund":
                    performanceData = RandomIntegerData.LowCapEquityFundPerformance;
                    break;
                case "Mid-Cap Balanced Fund":
                    performanceData = RandomIntegerData.MidCapBalancedFundPerformance;
                    break;
                case "Mid-Cap Debt Fund":
                    performanceData = RandomIntegerData.MidCapDebtFundPerformance;
                    break;
                case "Mid-Cap Equity Fund":
                    performanceData = RandomIntegerData.MidCapEquityFundPerformance;
                    break;
                default:
                    performanceData = new List<int>(); // Return an empty list for unknown fund types
                    break;
            }

            return Json(performanceData); // Return the performance data as JSON
        }

        // GET: mutual-funds/filter (Route: /mutual-funds/filter)
        [HttpGet]
        [Route("filter")]
        public IActionResult Filter(string fundType, string riskCategory, string assetClass)
        {
            // Query the database to get all mutual funds
            var mutualFunds = _context.MutualFunds.AsQueryable();

            // Apply filters based on the provided query parameters
            if (!string.IsNullOrEmpty(fundType))
            {
                mutualFunds = mutualFunds.Where(mf => mf.FundType == fundType);
            }

            if (!string.IsNullOrEmpty(riskCategory))
            {
                mutualFunds = mutualFunds.Where(mf => mf.RiskCategory == riskCategory);
            }

            if (!string.IsNullOrEmpty(assetClass))
            {
                mutualFunds = mutualFunds.Where(mf => mf.AssetClass == assetClass);
            }

            // Project the filtered results into a new anonymous object and return as JSON
            var filteredFunds = mutualFunds.Select(mf => new
            {
                mf.FundId,
                mf.FundName,
                mf.FundType,
                mf.RiskCategory,
                mf.AssetClass,
                mf.Nav,
                mf.ExpenseRatio,
                mf.Aum,
                mf.ReturnPercentage
            }).ToList();

            return Json(filteredFunds);
        }

        // GET: mutual-funds/details/{id} (Route: /mutual-funds/details/{id})
        [HttpGet]
        [Route("details/{id?}")]
        public IActionResult Details(long? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details requested with null id.");
                return NotFound(); // Return 404 Not Found if the ID is null
            }

            try
            {
                // Retrieve a specific mutual fund from the database by its ID
                var mutualFund = _context.MutualFunds.FirstOrDefault(m => m.FundId == id);
                if (mutualFund == null)
                {
                    _logger.LogWarning($"Mutual fund with id {id} not found.");
                    return NotFound(); // Return 404 Not Found if the mutual fund is not found
                }

                var viewModel = new MutualFundOrderViewModel
                {
                    MutualFund = mutualFund,
                    OrderType = "Lump Sum" // Set default order type
                };

                _logger.LogInformation($"Retrieved details for mutual fund with id {id}.");
                return View(viewModel); // Pass the data to the view for rendering
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving mutual fund details.");
                return StatusCode(500, "Internal server error"); // Return 500 error response in case of failure
            }
        }

        // GET: mutual-funds/invest/{id} (Route: /mutual-funds/invest/{id})
        [HttpGet]
        [Route("invest/{id?}")]
        public IActionResult Invest(long? id)
        {
            try
            {
                // Retrieve the mutual fund details for investment by ID
                var mutualFund = _context.MutualFunds.Find(id);
                if (mutualFund == null)
                {
                    _logger.LogWarning($"Invest page requested for mutual fund with id {id} but it was not found.");
                    return NotFound(); // Return 404 Not Found if the mutual fund is not found
                }

                var viewModel = new MutualFundOrderViewModel
                {
                    MutualFund = mutualFund
                };

                _logger.LogInformation($"Retrieved invest page for mutual fund with id {id}.");
                return View(viewModel); // Pass the data to the view for rendering
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving invest page.");
                return StatusCode(500, "Internal server error"); // Return 500 error response in case of failure
            }
        }

        // POST: mutual-funds/invest/{id} (Route: /mutual-funds/invest/{id})
        [HttpPost]
        [Route("invest/{id}")]
        [ValidateAntiForgeryToken] // Prevent CSRF attacks
        public async Task<IActionResult> Invest(long id, [Bind("OrderType,Amount,Frequency,SipDate")] MutualFundOrderViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invest action: Invalid model state. Validation errors:");

                    // Log validation errors
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning(" - {ErrorMessage}", error.ErrorMessage);
                    }

                    var mutualFund = _context.MutualFunds.Find(id);
                    if (mutualFund != null)
                    {
                        viewModel.MutualFund = mutualFund;
                    }
                    else
                    {
                        _logger.LogWarning($"Invest action: Mutual fund with id {id} was not found.");
                    }

                    return View(viewModel); // Return to the view with validation errors
                }

                // Retrieve the mutual fund to invest in by ID
                var mutualFundToInvest = _context.MutualFunds.Find(id);
                if (mutualFundToInvest == null)
                {
                    _logger.LogWarning($"Invest action: Mutual fund with id {id} was not found.");
                    return NotFound(); // Return 404 Not Found if the mutual fund is not found
                }

                // Create a new UserFund entry
                var userFund = new UserFund
                {
                    FundId = id,
                    UserId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier)), // Get the user ID from the claims
                    InvestedAmount = 0, // Initial invested amount
                    CurrentValue = 0, // Initial current value
                    Units = 0 // Initial units
                };

                _context.UserFunds.Add(userFund); // Add the new UserFund to the database
                _context.SaveChanges(); // Save changes to generate FolioNumber

                _logger.LogInformation("UserFund created with FolioNumber: {FolioNumber}", userFund.FolioNumber);

                // Create an Order based on the investment details
                var order = new Order
                {
                    Timestamp = DateTime.Now, // Set the timestamp of the order
                    FundId = id,
                    UserId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier)), // Get the user ID from the claims
                    FolioNumber = userFund.FolioNumber,
                };

                if (viewModel.OrderType == "Lump Sum")
                {
                    if (!viewModel.Amount.HasValue)
                    {
                        ModelState.AddModelError("Amount", "Amount is required for Lump Sum investments.");
                        _logger.LogWarning("Invest action: Amount is required for Lump Sum investments.");
                        viewModel.MutualFund = mutualFundToInvest;
                        return View(viewModel); // Return to the view with validation errors
                    }

                    // Set the order details for Lump Sum investment
                    order.Amount = viewModel.Amount.Value;
                    order.UnitPrice = mutualFundToInvest.Nav; // NAV (Net Asset Value) for the mutual fund
                    order.Units = order.Amount / mutualFundToInvest.Nav; // Calculate units based on amount and NAV
                    order.TransactionType = "Buy";

                    _context.Orders.Add(order); // Add the new Order to the database
                    _logger.LogInformation("Lump Sum Order created: {Order}", order);

                    // Update UserFund with the investment details
                    userFund.InvestedAmount += order.Amount;
                    userFund.CurrentValue += order.Amount;
                    userFund.Units += order.Units;
                }
                else if (viewModel.OrderType == "SIP")
                {
                    if (!viewModel.Amount.HasValue)
                    {
                        ModelState.AddModelError("Amount", "Amount is required for SIP investments.");
                        _logger.LogWarning("Invest action: Amount is required for SIP investments.");
                        viewModel.MutualFund = mutualFundToInvest;
                        return View(viewModel); // Return to the view with validation errors
                    }

                    // Create a new SIP entry
                    var sip = new SIP
                    {
                        FundId = id,
                        SipAmount = viewModel.Amount.Value,
                        Frequency = viewModel.Frequency,
                        InstallmentDate = viewModel.SipDate.Value, // Next installment date
                        StartDate = DateTime.Now,
                        Status = "Active",
                        FolioNumber = userFund.FolioNumber,
                        UserId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier)) // Get the user ID from the claims
                    };

                    _context.SIPs.Add(sip); // Add the new SIP to the database
                    _logger.LogInformation("SIP created: {SIP}", sip);

                    // Update UserFund for SIP investment
                    userFund.InvestedAmount = 0;
                    userFund.CurrentValue += 0; // Update current value if necessary
                }

                /* int userId = userFund*//* the UserId you are looking for *//*;
                 var email = _context.UserDetails
                                     .Where(u => u.UserId == userId)
                                     .Select(u => u.Email)
                                     .FirstOrDefault();*/

                var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Authentications.FindAsync(long.Parse(userid));
                // Send confirmation email
                var subject = "Amount Invested Successfully";
                var message = $"Hi ,<br/><br/>Your investement has been successfull for the folio number {userFund.FolioNumber}.<br/><br/>Thank you.";
                await _emailService.SendEmailAsync(user.Email, subject, message);

                _context.SaveChanges(); // Save changes to the database
                _logger.LogInformation($"Investment successfully processed for mutual fund with id {id}.");
                return RedirectToAction("UserOrders","Orders", new { id = user.UserId }); // Redirect to the index page after successful investment
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the investment.");
                return StatusCode(500, "Internal server error"); // Return 500 error response in case of failure
            }
        }
    }
}
