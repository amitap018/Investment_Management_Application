using Login.Data;
using Login.Models;
using Microsoft.EntityFrameworkCore;

namespace Login.Services
{
    public class NavUpdateScheduler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavUpdateScheduler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateNavValues();
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }

        private async Task UpdateNavValues()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var today = DateTime.Today;
                int dayOfMonth = today.Day; // Assuming the index corresponds to the day of the month

                // Update NAV values for each mutual fund based on the performance data
                var mutualFunds = context.MutualFunds.Include(mf => mf.UserFunds).ToList();

                foreach (var fund in mutualFunds)
                {
                    decimal performanceRate = 0;
                    switch (fund.FundName)
                    {
                        case "Balanced Fund":
                            performanceRate = RandomIntegerData.BalancedFundPerformance[dayOfMonth % 31];
                            break;
                        case "Global Debt Fund":
                            performanceRate = RandomIntegerData.GlobalDebtFundPerformance[dayOfMonth % 31];
                            break;
                        case "Global Equity Fund":
                            performanceRate = RandomIntegerData.GlobalEquityFundPerformance[dayOfMonth % 31];
                            break;
                        case "High-Cap Balanced Fund":
                            performanceRate = RandomIntegerData.HighCapBalancedFundPerformance[dayOfMonth % 31];
                            break;
                        case "High-Cap Debt Fund":
                            performanceRate = RandomIntegerData.HighCapDebtFundPerformance[dayOfMonth % 31];
                            break;
                        case "Low-Cap Debt Fund":
                            performanceRate = RandomIntegerData.LowCapDebtFundPerformance[dayOfMonth % 31];
                            break;
                        case "Low-Cap Equity Fund":
                            performanceRate = RandomIntegerData.LowCapEquityFundPerformance[dayOfMonth % 31];
                            break;
                        case "Mid-Cap Balanced Fund":
                            performanceRate = RandomIntegerData.MidCapBalancedFundPerformance[dayOfMonth % 31];
                            break;
                        case "Mid-Cap Debt Fund":
                            performanceRate = RandomIntegerData.MidCapDebtFundPerformance[dayOfMonth % 31];
                            break;
                        case "Mid-Cap Equity Fund":
                            performanceRate = RandomIntegerData.MidCapEquityFundPerformance[dayOfMonth % 31];
                            break;
                    }

                    fund.Nav += fund.Nav * (performanceRate / 100);
                    foreach (var userFund in fund.UserFunds)
                    {
                        userFund.CurrentValue = userFund.Units * fund.Nav;
                        userFund.PAndL = userFund.CurrentValue - userFund.InvestedAmount;
                        userFund.OneDayChange = performanceRate;
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}