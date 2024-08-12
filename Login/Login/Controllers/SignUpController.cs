using Login.Data;
using Login.Models;
using Login.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Login.Controllers
{
  
    public class SignUpController : Controller
    {
        private readonly ApplicationDbContext _context; // Database context
        private readonly ILogger<SignUpController> _logger; // Logger for logging information
        private readonly IEmailService _emailService; // Email service for sending emails

        public SignUpController(ApplicationDbContext context, ILogger<SignUpController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View(); // Return the SignUp view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(Authentication model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if the email already exists
                    var existingUser = await _context.Authentications.FirstOrDefaultAsync(u => u.Email == model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Email already exists.");
                        return View(model); // Return the same view with the model
                    }

                    // Hash the password
                    var hashedPassword = HashingHelper.ComputeSha256Hash(model.Password);

                    // Create a new Authentication object
                    var auth = new Authentication
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Password = hashedPassword
                    };

                    _context.Add(auth); // Add the new user to the context
                    await _context.SaveChangesAsync(); // Save changes to the database

                    _logger.LogInformation("User signed up successfully.");
                    return RedirectToAction(nameof(Register), new { id = auth.UserId }); // Redirect to the Register action
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during sign-up.");
                    ModelState.AddModelError(string.Empty, "An error occurred while signing up. Please try again.");
                }
            }
            return View(model); // Return the same view if the model state is invalid
        }

        [HttpGet]
        public async Task<IActionResult> Register(long id)
        {
            try
            {
                var user = await _context.Authentications.FindAsync(id); // Find the user by ID
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId}", id);
                    return NotFound(); // Return 404 if user not found
                }

                // Create a UserDetails object with the user's information
                var userDetails = new UserDetails
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email
                };

                return View(userDetails); // Return the Register view with the userDetails model
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading register page.");
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserDetails model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Generate a unique Demat ID for the user
                    model.DematId = GenerateDematId();
                    _context.Add(model); // Add the user details to the context
                    await _context.SaveChangesAsync(); // Save changes to the database
                    _logger.LogInformation("User registered successfully.");

                    // Send a confirmation email to the user
                    var subject = "Account Successfully Created";
                    var message = $"Dear {model.Name},<br/><br/>Your account has been successfully created. Your Demat ID is {model.DematId}.<br/><br/>Thank you.";
                    await _emailService.SendEmailAsync(model.Email, subject, message);

                    // Create authentication claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, model.UserId.ToString()),
                        new Claim(ClaimTypes.Name, model.Name)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Dashboard", new { id = model.UserId }); // Redirect to Dashboard
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during registration.");
                    ModelState.AddModelError(string.Empty, "An error occurred while registering. Please try again.");
                }
            }
            return View(model); // Return the same view if the model state is invalid
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            try
            {
                var user = await _context.UserDetails.FindAsync(id); // Find the user details by ID
                if (user == null)
                {
                    return NotFound(); // Return 404 if user not found
                }

                return View(user); // Return the Details view with the user model
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading details page.");
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Return the Login view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AuthenticationLogin model)
        {
            _logger.LogInformation("Login attempt for email: {Email}", model.Email);

            if (ModelState.IsValid)
            {
                try
                {
                    // Hash the entered password
                    var hashedPassword = HashingHelper.ComputeSha256Hash(model.Password);

                    // Find the user with the entered email and hashed password
                    var user = await _context.Authentications.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == hashedPassword);
                    if (user != null)
                    {
                        // Create authentication claims
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                            new Claim(ClaimTypes.Name, user.Name),
                            new Claim(ClaimTypes.Email, user.Email)
                        };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        _logger.LogInformation("User logged in successfully.");
                        return RedirectToAction("Index", "Dashboard", new { id = user.UserId }); // Redirect to Dashboard
                    }

                    _logger.LogWarning("Invalid login attempt for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during login.");
                    ModelState.AddModelError(string.Empty, "An error occurred while logging in. Please try again.");
                }
            }
            return View(model); // Return the same view if the model state is invalid
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Sign out the user
                _logger.LogInformation("User logged out successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout.");
            }
            return RedirectToAction(nameof(Login)); // Redirect to Login
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordModel()); // Return the ForgotPassword view with a new model
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Return the same view if the model state is invalid
            }

            try
            {
                // Find the user with the entered email
                var user = await _context.Authentications.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("Email", "User with this email does not exist.");
                    return View(model); // Return the same view if user not found
                }

                // Generate OTP code and expiration time
                var otpCode = GenerateOtpCode();
                var expirationTime = DateTime.Now.AddMinutes(3);

                // Create a new OTP object
                var otp = new Otp
                {
                    Email = model.Email,
                    Code = otpCode,
                    ExpirationTime = expirationTime
                };
                _context.Otps.Add(otp); // Add the OTP to the context
                await _context.SaveChangesAsync(); // Save changes to the database

                // Send OTP email to the user
                var subject = "Your OTP Code";
                var message = $"Your OTP code is {otpCode}. It is valid for 3 minutes.";
                await _emailService.SendEmailAsync(model.Email, subject, message);

                _logger.LogInformation("OTP sent to {Email}", model.Email);
                return RedirectToAction(nameof(VerifyOtp), new { email = model.Email }); // Redirect to VerifyOtp
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing forgot password.");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            var model = new VerifyOtpModel { Email = email }; // Create a new VerifyOtpModel with the email
            return View(model); // Return the VerifyOtp view with the model
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Find the OTP with the entered email and code
                    var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == model.Email && o.Code == model.Code);
                    if (otp != null && otp.ExpirationTime > DateTime.Now)
                    {
                        return RedirectToAction(nameof(ResetPassword), new { email = model.Email }); // Redirect to ResetPassword if OTP is valid
                    }

                    ModelState.AddModelError("Code", "Invalid or expired OTP.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during OTP verification.");
                    ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                }
            }
            return View(model); // Return the same view if the model state is invalid
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            var model = new ResetPasswordModel { Email = email }; // Create a new ResetPasswordModel with the email
            return View(model); // Return the ResetPassword view with the model
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Find the user with the entered email
                    var user = await _context.Authentications.FirstOrDefaultAsync(u => u.Email == model.Email);
                    if (user != null)
                    {
                        // Update the user's password
                        user.Password = HashingHelper.ComputeSha256Hash(model.NewPassword);
                        await _context.SaveChangesAsync(); // Save changes to the database

                        // Remove the OTP after password reset
                        var otp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == model.Email);
                        if (otp != null)
                        {
                            _context.Otps.Remove(otp);
                            await _context.SaveChangesAsync(); // Save changes to the database
                        }

                        _logger.LogInformation("Password reset successfully for {Email}", model.Email);
                        return RedirectToAction(nameof(Login)); // Redirect to Login
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during password reset.");
                    ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                }
            }
            return View(model); // Return the same view if the model state is invalid
        }

        // Method to generate a unique Demat ID
        private string GenerateDematId()
        {
            var random = new Random();
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var prefix = new string(Enumerable.Range(0, 2).Select(_ => letters[random.Next(letters.Length)]).ToArray());
            var number = random.Next(1000, 10000).ToString("D4");
            return $"{prefix}{number}";
        }

        // Method to generate a random OTP code
        private string GenerateOtpCode()
        {
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();
            return otpCode;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user's ID

            if (userId == null)
            {
                _logger.LogWarning("User is not logged in.");
                TempData["ErrorMessage"] = "User is not logged in.";
                return RedirectToAction("UserDetails"); // Redirect to UserDetails if user is not logged in
            }

            try
            {
                // Find the user by ID
                var user = await _context.Authentications.FindAsync(long.Parse(userId));
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId}", userId);
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("UserDetails"); // Redirect to UserDetails if user not found
                }

                try
                {
                    // Find all user funds related to the user
                    var userFunds = _context.UserFunds.Where(uf => uf.UserId == user.UserId);

                    foreach (var userFund in userFunds)
                    {
                        // Find and remove all SIPs related to the user funds
                        var relatedSIPs = await _context.SIPs.Where(s => s.FolioNumber == userFund.FolioNumber).ToListAsync();

                        if (relatedSIPs.Any())
                        {
                            _context.SIPs.RemoveRange(relatedSIPs);
                        }
                    }

                    foreach (var userFund in userFunds)
                    {
                        // Find and remove all orders related to the user funds
                        var orders = _context.Orders.Where(o => o.FolioNumber == userFund.FolioNumber);
                        _context.Orders.RemoveRange(orders);
                    }

                    _context.UserFunds.RemoveRange(userFunds); // Remove all user funds

                    // Find and remove the user's details
                    var userDetails = await _context.UserDetails.FirstOrDefaultAsync(ud => ud.UserId == user.UserId);
                    if (userDetails != null)
                    {
                        _context.UserDetails.Remove(userDetails);
                    }

                    _context.Authentications.Remove(user); // Remove the user
                    await _context.SaveChangesAsync(); // Save changes to the database

                    _logger.LogInformation("User deleted successfully.");

                    // Send account closure email
                    var subject = "Account Closure";
                    var message = $"Your account has been successfully closed. All funds will be transferred to your bank account.";
                    await _emailService.SendEmailAsync(user.Email, subject, message);
                    _logger.LogInformation("Email sent to {Email}", user.Email);

                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Sign out the user
                    _logger.LogInformation("User logged out successfully.");

                    TempData["SuccessMessage"] = "Account deleted successfully.";
                    return RedirectToAction("Login", "SignUp"); // Redirect to Login
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during account deletion.");
                    TempData["ErrorMessage"] = "An error occurred while deleting your account. Please try again.";
                    return RedirectToAction("UserDetails"); // Redirect to UserDetails if an error occurs
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finding user for account deletion.");
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
                return RedirectToAction("UserDetails"); // Redirect to UserDetails if an error occurs
            }
        }
    }
}
