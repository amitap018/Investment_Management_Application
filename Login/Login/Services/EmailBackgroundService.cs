using Microsoft.EntityFrameworkCore;
using Login.Data;
using Login.Models;
using Login.Services;

namespace Login.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEmailService _emailService;

        public EmailBackgroundService(IServiceProvider serviceProvider, IEmailService emailService)
        {
            _serviceProvider = serviceProvider;
            _emailService = emailService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Get the current date
                    var currentDate = DateTime.Now.Date;

                    var activeSIPs = await context.SIPs
                        .Include(s => s.UserDetails)
                        .Include(s => s.UserFund)
                        .Include(s => s.MutualFund)
                        .Where(s => s.Status == "Active")
                        .ToListAsync();

                    foreach (var sip in activeSIPs)
                    {
                        var user = sip.UserDetails;
                        var userFund = sip.UserFund;

                        if (user != null && userFund != null && sip.InstallmentDate.Date <= currentDate)
                        {
                            // Calculate next installment date based on frequency
                            DateTime nextInstallmentDate;
                            if (sip.Frequency == "Daily")
                            {
                                nextInstallmentDate = sip.InstallmentDate.AddDays(1);
                            }
                            else if (sip.Frequency == "Weekly")
                            {
                                nextInstallmentDate = sip.InstallmentDate.AddDays(7);
                            }
                            else
                            {
                                // Handle other frequencies as needed
                                continue;
                            }

                            // Get the NAV (Net Asset Value) of the mutual fund
                            var nav = sip.MutualFund.Nav;
                            var units = sip.SipAmount / nav;

                            // Create new order
                            var order = new Order
                            {
                                UserId = user.UserId,
                                FundId = sip.FundId,
                                FolioNumber = userFund.FolioNumber,
                                Units = units,
                                UnitPrice = nav,
                                Amount = sip.SipAmount,
                                IsSip = true,
                                TransactionType = "Buy",
                                Timestamp = DateTime.Now
                            };

                            context.Orders.Add(order);

                            // Update InvestedAmount and Units in UserFund
                            userFund.InvestedAmount += sip.SipAmount;
                            userFund.CurrentValue += sip.SipAmount;
                            userFund.Units += units;

                            // Update SIP's InstallmentDate
                            sip.InstallmentDate = nextInstallmentDate;

                            // Send email
                            string subject = "SIP Investment Update";
                            string body = $"Dear {user.Name},\n\nYour SIP investment has been processedat {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}. " +
                                          $"Amount: {sip.SipAmount} has been added to your invested amount. " +
                                          $"New units: {units}.\n\nThank you.";
                            _emailService.SendEmailAsync(user.Email, subject, body);

                            context.UserFunds.Update(userFund);
                            context.SIPs.Update(sip);
                        }
                    }

                    await context.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(0), stoppingToken); // Run every 5 minutes
            }
        }
    }
}
