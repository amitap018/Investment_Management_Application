using Login.Data;
using Login.Models;
using Login.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Login.Controllers
{

    [Authorize]
    public class CustomerSupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerSupportController> _logger;
        private readonly IEmailService _emailService;

        // Constructor to inject dependencies
        public CustomerSupportController(ApplicationDbContext context, ILogger<CustomerSupportController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        // GET method to render the feedback submission view
        [HttpGet]
        public IActionResult SubmitFeedback()
        {
            return View();
        }

        // POST method to handle feedback form submission
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevent CSRF attacks
        public async Task<IActionResult> SubmitFeedback(Feedback model)
        {
            _logger.LogInformation("Feedback submission attempt by user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (ModelState.IsValid) // Check if model state is valid
            {
                try
                {
                    model.UserId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    model.CreatedAt = DateTime.Now; // Set the timestamp

                    _context.Feedbacks.Add(model); // Add feedback to the database
                    await _context.SaveChangesAsync(); // Save changes to the database

                    _logger.LogInformation("Feedback submitted successfully.");

                    // Send confirmation email
                    var user = await _context.UserDetails.FindAsync(model.UserId);
                    var subject = "Feedback Submitted Successfully";
                    var message = $"Dear {user.Name},<br/><br/>Thank you for your feedback!<br/><br/>Rating: {model.Rating}<br/>Description: {model.Description}<br/><br/>We appreciate your input.";
                    await _emailService.SendEmailAsync(user.Email, subject, message);

                    TempData["FeedbackSubmission"] = "Success";
                    return RedirectToAction("SubmitFeedback");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during feedback submission.");
                    ModelState.AddModelError(string.Empty, "An error occurred while submitting feedback. Please try again.");
                    TempData["FeedbackSubmission"] = "Error";
                }
            }
            else
            {
                _logger.LogWarning("Invalid model state for feedback submission by user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Model state error: {ErrorMessage}", error.ErrorMessage);
                }

                TempData["FeedbackSubmission"] = "Error";
            }

            return RedirectToAction("SubmitFeedback");
        }

        // GET method to render the grievance submission view
        [HttpGet]
        public IActionResult SubmitGrievance()
        {
            return View();
        }

        // POST method to handle grievance form submission
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevent CSRF attacks
        public async Task<IActionResult> SubmitGrievance(Grievance model)
        {
            _logger.LogInformation("Grievance submission attempt by user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (ModelState.IsValid) // Check if model state is valid
            {
                try
                {
                    model.UserId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    model.CreatedAt = DateTime.Now; // Set the timestamp

                    _context.Grievances.Add(model); // Add grievance to the database
                    await _context.SaveChangesAsync(); // Save changes to the database

                    _logger.LogInformation("Grievance submitted successfully.");

                    // Send confirmation email
                    var user = await _context.UserDetails.FindAsync(model.UserId);
                    var subject = "Grievance Submitted Successfully";
                    var message = $"Dear {user.Name},<br/><br/>Thank you for submitting your grievance.<br/><br/>Subject: {model.Subject}<br/>Description: {model.Description}<br/><br/>We will address it shortly.";
                    await _emailService.SendEmailAsync(user.Email, subject, message);

                    return RedirectToAction("UserGrievances");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during grievance submission.");
                    ModelState.AddModelError(string.Empty, "An error occurred while submitting grievance. Please try again.");
                }
            }
            else
            {
                _logger.LogWarning("Invalid model state for grievance submission by user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Model state error: {ErrorMessage}", error.ErrorMessage);
                }
            }

            return View(model);
        }

        // GET method to retrieve and display user grievances
        [HttpGet]
        public async Task<IActionResult> UserGrievances()
        {
            try
            {
                var userId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var grievances = await _context.Grievances
                    .Where(g => g.UserId == userId) // Filter grievances by user ID
                    .OrderByDescending(g => g.CreatedAt) // Order by creation date descending
                    .ToListAsync(); // Execute query asynchronously

                return View(grievances); // Return view with grievances
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user grievances.");
                return RedirectToAction("SubmitGrievance");
            }
        }
    }
}

