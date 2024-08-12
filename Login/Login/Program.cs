
using System.IO; // New Code
using Login.Services;
using Login.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog; // New Code
using Serilog.Events; // New Code

namespace Login
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create a builder
            var builder = WebApplication.CreateBuilder(args);

            // New Code: Ensure log directory exists
            var logFilePath = Path.Combine("Logs", "app-.txt");
            var logDirectory = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // New Code: Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    formatter: new Serilog.Formatting.Compact.CompactJsonFormatter())
                .CreateLogger();

            // New Code: Use Serilog for logging
            builder.Host.UseSerilog();

            // Add services to the container
            builder.Services.AddControllersWithViews(); // Add MVC services to the container

            // Add DbContext for SQL Server using the DefaultConnection connection string
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // Configure DbContext with SQL Server

            // Register hosted services
            builder.Services.AddHostedService<EmailBackgroundService>(); // Background service for handling email operations
            builder.Services.AddHostedService<NavUpdateScheduler>(); // Background service for updating navigation

            builder.Services.AddSingleton<EmailService>(); // Register EmailService as a singleton

            // Add database exception filter for developer pages
            builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // Exception filter for developer pages

            // Add authentication services with cookie authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/SignUp/Login"; // Path for login
                    options.LogoutPath = "/SignUp/Logout"; // Path for logout
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5); // Cookie expiration time
                    options.SlidingExpiration = true; // Enable sliding expiration
                });

            // Clear default logging providers and add console logging
            //builder.Logging.ClearProviders(); // Clear default logging providers
            //builder.Logging.AddConsole(); // Add console logging

            // New Code: Clear default logging providers and add Serilog
            builder.Logging.ClearProviders(); // Clear default logging providers
            builder.Logging.AddSerilog(); // Add Serilog logging

            // Register IHttpContextAccessor for accessing HTTP context
            builder.Services.AddHttpContextAccessor(); // Register IHttpContextAccessor

            // Register EmailService for dependency injection
            builder.Services.AddTransient<IEmailService, EmailService>(); // Register EmailService as transient

            var app = builder.Build();

            // Create the database if it does not exist
            CreateDbIfNotExists(app); // Ensure the database is created

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error"); // Use custom error page in non-development environments
                app.UseHsts(); // Use HSTS (HTTP Strict Transport Security)
            }

            app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
            app.UseStaticFiles(); // Serve static files

            app.UseRouting(); // Enable routing

            app.UseAuthentication(); // Enable authentication
            app.UseAuthorization(); // Enable authorization

            // Map default controller route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=SignUp}/{action=Login}/{id?}"); // Set default route to SignUp controller's Login action

            app.Run(); // Run the application
        }

        // Method to create the database if it does not exist
        private static void CreateDbIfNotExists(IHost app)
        {
            using (var scope = app.Services.CreateScope()) // Create a scope to obtain services
            {
                var services = scope.ServiceProvider; // Get service provider
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>(); // Get DbContext instance
                    DbInitializer.Initialize(context); // Initialize the database
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>(); // Get logger service
                    logger.LogError(ex, "An error occurred creating the DB."); // Log any errors during DB creation
                }
            }
        }
    }
}
