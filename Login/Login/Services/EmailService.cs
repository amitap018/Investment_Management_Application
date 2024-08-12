using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Login.Services
{
    public interface IEmailService
    {
        // Interface method to send an email asynchronously
        Task SendEmailAsync(string toEmail, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        // Constructor to initialize configuration and logger dependencies
        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Method to send an email asynchronously using SMTP
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                // Create SMTP client with configuration settings
                var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
                {
                    Port = int.Parse(_configuration["EmailSettings:SmtpPort"]),
                    Credentials = new NetworkCredential(_configuration["EmailSettings:SmtpUsername"], _configuration["EmailSettings:SmtpPassword"]),
                    EnableSsl = true,
                };

                // Create email message with provided parameters
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["EmailSettings:SmtpUsername"]),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                // Send email asynchronously
                await smtpClient.SendMailAsync(mailMessage);

                // Log success message
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (SmtpException smtpEx)
            {
                // Log SMTP-specific errors
                _logger.LogError(smtpEx, "SMTP Error sending email to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                // Log general errors
                _logger.LogError(ex, "Error sending email to {Email}", toEmail);
            }
        }
    }
}
