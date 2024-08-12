using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Login.Data;
using Login.Models;
using System.Security.Claims;
using Newtonsoft.Json;
using Login.Services;

namespace Investment2.Controllers
{
    public class DashboardController : Controller
    {
        // Dependency injection to get the database context and email service instances
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        // Constructor to initialize dependencies
        public DashboardController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Dashboard
        // Main action to display the user's dashboard with investment details
        public async Task<IActionResult> Index()
        {
            // Retrieve the current user's ID from the authentication claims
            long userId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Fetch the active user funds for the current user using LINQ and EF Core
            var userFunds = await _context.UserFunds
                .Include(uf => uf.MutualFund) // EF Core include to perform join and fetch related MutualFund data
                .Where(uf => uf.UserId == userId && uf.Status == Status.Active) // LINQ to filter active funds
                .ToListAsync(); // Asynchronously convert the result to a list

            // Calculate the total investment, total profit & loss, and portfolio amount
            var totalInvestment = userFunds.Sum(uf => uf.InvestedAmount);
            var totalPandL = userFunds.Sum(uf => uf.PAndL);
            var portfolioAmount = totalInvestment + totalPandL;

            // Store the calculated values in ViewBag to pass them to the view
            ViewBag.TotalInvestment = totalInvestment;
            ViewBag.TotalPandL = totalPandL;
            ViewBag.PortfolioAmount = portfolioAmount;
            ViewBag.SelectedUserId = userId;
            ViewBag.UserFunds = userFunds;
            ViewBag.UserSIPs = await _context.SIPs
                .Where(s => s.UserId == userId)
                .ToListAsync();

            // Set initial performance data for chart
            ViewBag.PerformanceData = JsonConvert.SerializeObject(RandomIntegerData.BalancedFundPerformance);

            // Pass success message from TempData to ViewBag
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

            return View();
        }

        // GET: Dashboard/GetFundPerformance
        // Action to return performance data of a specific mutual fund
        [HttpGet]
        public IActionResult GetFundPerformance(string fund)
        {
            // Switch case to return performance data based on the fund name
            List<int> performanceData;
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
                    performanceData = new List<int>();
                    break;
            }

            return Json(performanceData);
        }

        // GET: Dashboard/OneTimeInvestment
        // Action to display the One Time Investment page for a specific fund
        public async Task<IActionResult> OneTimeInvestment(long folioNumber)
        {
            // Fetch user fund details for the given folio number using EF Core
            var userFund = await _context.UserFunds
                .Include(uf => uf.MutualFund) // Join with MutualFund table
                .FirstOrDefaultAsync(uf => uf.FolioNumber == folioNumber);

            if (userFund == null)
            {
                return NotFound();
            }

            // Pass necessary data to the view using ViewBag
            ViewBag.FolioNumber = folioNumber;
            ViewBag.FundName = userFund.MutualFund.FundName;
            ViewBag.UnitPrice = userFund.MutualFund.Nav;
            return View();
        }

        // GET: Dashboard/Details
        // Action to display the details of a specific investment
        public async Task<IActionResult> Details(long folioNumber)
        {
            // Fetch the user fund details along with related MutualFund and UserDetails
            var userFund = await _context.UserFunds
                .Include(uf => uf.MutualFund)
                .Include(uf => uf.UserDetails)
                .FirstOrDefaultAsync(m => m.FolioNumber == folioNumber);

            if (userFund == null)
            {
                return NotFound();
            }

            return View(userFund);
        }

        // POST: Dashboard/OneTimeInvestment
        // Action to handle the submission of a one-time investment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OneTimeInvestment(long folioNumber, decimal amount)
        {
            // Fetch user fund details for the given folio number
            var userFund = await _context.UserFunds
                .Include(uf => uf.MutualFund)
                .FirstOrDefaultAsync(uf => uf.FolioNumber == folioNumber);

            if (userFund == null)
            {
                return NotFound();
            }

            var mutualFund = userFund.MutualFund;
            var units = amount / mutualFund.Nav; // Calculate units based on the NAV

            // Update the invested amount in the UserFund table
            userFund.InvestedAmount += amount;

            // Create a new order for the investment
            var order = new Order
            {
                Timestamp = DateTime.Now,
                Units = units,
                UnitPrice = mutualFund.Nav,
                Amount = amount,
                FundId = mutualFund.FundId,
                UserId = userFund.UserId,
                TransactionType = "Buy",
                FolioNumber = folioNumber
            };

            _context.Orders.Add(order);
            _context.UserFunds.Update(userFund);
            await _context.SaveChangesAsync();

            // Set success message in TempData to be displayed after redirect
            TempData["SuccessMessage"] = $"Order placed successfully with OrderId {order.OrderId}.";

            return RedirectToAction(nameof(Index), new { userId = userFund.UserId });
        }

        // GET: Dashboard/StartOrEditSIP
        // Action to display the Start or Edit SIP page
        public async Task<IActionResult> StartOrEditSIP(long folioNumber)
        {
            // Fetch user fund details for the given folio number
            var userFund = await _context.UserFunds
                .Include(uf => uf.MutualFund)
                .FirstOrDefaultAsync(uf => uf.FolioNumber == folioNumber);

            if (userFund == null)
            {
                return NotFound();
            }

            // Fetch existing SIP details if available
            var existingSIP = await _context.SIPs
                .FirstOrDefaultAsync(s => s.FolioNumber == folioNumber);

            ViewBag.FolioNumber = folioNumber;
            ViewBag.FundName = userFund.MutualFund.FundName;
            ViewBag.ExistingSIP = existingSIP;

            return View(existingSIP);
        }

        // POST: Dashboard/StartOrEditSIP
        // Action to handle the submission of starting or editing an SIP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartOrEditSIP(long folioNumber, decimal amount, string frequency)
        {
            // Fetch user fund details for the given folio number
            var userFund = await _context.UserFunds
                .Include(uf => uf.MutualFund)
                .FirstOrDefaultAsync(uf => uf.FolioNumber == folioNumber);

            if (userFund == null)
            {
                return NotFound();
            }

            // Fetch existing SIP details if available
            var existingSIP = await _context.SIPs
                .FirstOrDefaultAsync(s => s.FolioNumber == folioNumber);

            if (existingSIP != null)
            {
                // Update existing SIP details
                existingSIP.SipAmount = amount;
                existingSIP.Frequency = frequency;
                existingSIP.InstallmentDate = DateTime.Now;
                _context.Update(existingSIP);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "SIP updated successfully.";

                // Send email notification for SIP update
                var user = await _context.UserDetails.FindAsync(existingSIP.UserId);
                var subject = "SIP Updated Successfully";
                var message = $"Dear {user.Name},<br/><br/>Your SIP has been updated successfully.<br/><br/>Fund: {userFund.MutualFund.FundName}<br/>Amount: ₹{amount}<br/>Frequency: {frequency}<br/><br/>Thank you for investing with us.";
                await _emailService.SendEmailAsync(user.Email, subject, message);
            }
            else
            {
                // Create a new SIP
                var newSIP = new SIP
                {
                    FundId = userFund.FundId,
                    SipAmount = amount,
                    Frequency = frequency,
                    InstallmentDate = DateTime.Now,
                    StartDate = DateTime.Now,
                    Status = "Active",
                    FolioNumber = folioNumber,
                    UserId = userFund.UserId
                };

                _context.SIPs.Add(newSIP);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "SIP started successfully.";

                // Send email notification for new SIP
                var user = await _context.UserDetails.FindAsync(newSIP.UserId);
                var subject = "SIP Started Successfully";
                var message = $"Dear {user.Name},<br/><br/>Your SIP has been started successfully.<br/><br/>Fund: {userFund.MutualFund.FundName}<br/>Amount: ₹{amount}<br/>Frequency: {frequency}<br/><br/>Thank you for investing with us.";
                await _emailService.SendEmailAsync(user.Email, subject, message);
            }

            return RedirectToAction(nameof(Index), new { userId = userFund.UserId });
        }

        // GET: Dashboard/CancelSIP
        // Action to display the Cancel SIP page
        public async Task<IActionResult> CancelSIP(long folioNumber)
        {
            // Fetch the SIP details for the given folio number
            var sip = await _context.SIPs
                .FirstOrDefaultAsync(s => s.FolioNumber == folioNumber);

            if (sip == null)
            {
                return NotFound();
            }

            ViewBag.FolioNumber = folioNumber;
            return View(sip);
        }

        // POST: Dashboard/CancelSIP
        // Action to handle the submission of SIP cancellation
        [HttpPost, ActionName("CancelSIP")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelSIPConfirmed(long folioNumber)
        {
            // Fetch the SIP details for the given folio number
            var sip = await _context.SIPs
                .FirstOrDefaultAsync(s => s.FolioNumber == folioNumber);

            if (sip == null)
            {
                return NotFound();
            }

            _context.SIPs.Remove(sip);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "SIP canceled successfully.";

            // Send email notification for SIP cancellation
            var user = await _context.UserDetails.FindAsync(sip.UserId);
            var subject = "SIP Canceled Successfully";
            var message = $"Dear {user.Name},<br/><br/>Your SIP has been canceled successfully.<br/><br/>Fund: {sip.FundId}<br/>Folio Number: {folioNumber}<br/><br/>Thank you for investing with us.";
            await _emailService.SendEmailAsync(user.Email, subject, message);

            return RedirectToAction(nameof(Index), new { userId = sip.UserId });
        }

        // GET: Dashboard/Redeem
        // Action to display the Redeem page
        public async Task<IActionResult> Redeem(long folioNumber)
        {
            try
            {
                // Fetch user fund details for the given folio number
                var userFund = await _context.UserFunds
                    .Include(uf => uf.MutualFund)
                    .FirstOrDefaultAsync(uf => uf.FolioNumber == folioNumber);

                if (userFund == null)
                {
                    return NotFound();
                }

                // Fetch the most recent order for the given folio number
                var mostRecentOrder = await _context.Orders
                    .Where(o => o.FolioNumber == folioNumber)
                    .OrderByDescending(o => o.Timestamp)
                    .FirstOrDefaultAsync();

                if (mostRecentOrder == null)
                {
                    return NotFound("No transactions found for this folio number.");
                }

                var mutualFund = userFund.MutualFund;
                var investedAmount = userFund.InvestedAmount;
                var pAndL = userFund.PAndL;

                // Constants for tax and commission rates
                const decimal taxRate = 0.07m;
                const decimal commissionRate = 0.10m;

                var taxes = pAndL > 0 ? pAndL * taxRate : 0;
                var commission = pAndL > 0 ? pAndL * commissionRate : 0;

                var exitLoad = mutualFund.ExitLoad;
                var exitLoadPeriod = mutualFund.ExitLoadPeriod;
                var currentDate = DateTime.Now;
                var startDate = mostRecentOrder.Timestamp;

                // Calculate the exit period in months
                int monthsPassed = ((currentDate.Year - startDate.Year) * 12) + currentDate.Month - startDate.Month;
                if (currentDate.Day < startDate.Day)
                {
                    monthsPassed--;
                }

                // Set exit load value to zero if exit load period is zero or surpassed
                if (monthsPassed >= exitLoadPeriod)
                {
                    exitLoad = 0;
                }

                var exitLoadValue = (exitLoad / 100) * investedAmount;
                var amountToReceive = investedAmount + pAndL - taxes - commission - exitLoadValue;

                // Pass necessary data to the view using ViewBag
                ViewBag.InvestedAmount = investedAmount;
                ViewBag.PAndL = pAndL;
                ViewBag.Taxes = taxes;
                ViewBag.Commission = commission;
                ViewBag.ExitLoad = exitLoadValue;
                ViewBag.AmountToReceive = amountToReceive;
                ViewBag.FolioNumber = folioNumber;
                ViewBag.ExitPeriodMonths = exitLoadPeriod;

                return View(userFund);
            }
            catch (Exception ex)
            {
                // Log the exception (this is a placeholder, implement actual logging)
                Console.WriteLine(ex);

                // Return an error view or message
                ViewBag.ErrorMessage = "An error occurred while processing your request.";
                return View("Error");
            }
        }

        // POST: Dashboard/RedeemConfirmed
        // Action to handle the submission of redemption confirmation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedeemConfirmed(long folioNumber)
        {
            // Fetch user fund details for the given folio number
            var userFund = await _context.UserFunds
                .Include(uf => uf.MutualFund)
                .FirstOrDefaultAsync(uf => uf.FolioNumber == folioNumber);

            if (userFund == null)
            {
                return NotFound();
            }

            // Fetch the most recent order for the given folio number
            var mostRecentOrder = await _context.Orders
                .Where(o => o.FolioNumber == folioNumber)
                .OrderByDescending(o => o.Timestamp)
                .FirstOrDefaultAsync();

            if (mostRecentOrder == null)
            {
                return NotFound("No transactions found for this folio number.");
            }

            var mutualFund = userFund.MutualFund;
            var investedAmount = userFund.InvestedAmount;
            var pAndL = userFund.PAndL;

            // Constants for tax and commission rates
            const decimal taxRate = 0.07m;
            const decimal commissionRate = 0.10m;

            var taxes = pAndL > 0 ? pAndL * taxRate : 0;
            var commission = pAndL > 0 ? pAndL * commissionRate : 0;

            var exitLoad = mutualFund.ExitLoad;
            var exitLoadPeriod = mutualFund.ExitLoadPeriod;
            var currentDate = DateTime.Now;
            var startDate = mostRecentOrder.Timestamp;
            var monthsPassed = ((currentDate.Year - startDate.Year) * 12) + currentDate.Month - startDate.Month;
            if (currentDate.Day < startDate.Day)
            {
                monthsPassed--;
            }

            // Set exit load value to zero if exit load period is zero or surpassed
            if (monthsPassed >= exitLoadPeriod)
            {
                exitLoad = 0;
            }

            var exitLoadValue = (exitLoad / 100) * investedAmount;
            var amountToReceive = investedAmount + pAndL - taxes - commission - exitLoadValue;

            // Create a new order for the redemption
            var order = new Order
            {
                Timestamp = DateTime.Now,
                Units = userFund.Units,
                UnitPrice = mutualFund.Nav,
                Amount = amountToReceive,
                FundId = mutualFund.FundId,
                UserId = userFund.UserId,
                TransactionType = "Sell",
                FolioNumber = folioNumber
            };

            _context.Orders.Add(order);
            userFund.Status = Status.Inactive; // Set status to inactive after redemption
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Redemption successful with OrderId {order.OrderId}.";

            // Send email notification for redemption
            var user = await _context.UserDetails.FindAsync(userFund.UserId);
            var subject = "Redemption Successful";
            var message = $"Dear {user.Name},<br/><br/>Your redemption has been processed successfully.<br/><br/>Fund: {mutualFund.FundName}<br/>Folio Number: {folioNumber}<br/>Invested Amount: ₹{investedAmount}<br/>P&L: ₹{pAndL}<br/>Taxes: ₹{taxes}<br/>Commission: ₹{commission}<br/>Exit Load: ₹{exitLoadValue}<br/>Amount to Receive: ₹{amountToReceive}<br/><br/>Thank you for investing with us.";
            await _emailService.SendEmailAsync(user.Email, subject, message);

            return RedirectToAction(nameof(Index), new { userId = userFund.UserId });
        }
    }
}
