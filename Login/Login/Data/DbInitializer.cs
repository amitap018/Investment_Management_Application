using Login.Models;


namespace Login.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Ensure the database is created
            context.Database.EnsureCreated();

            // Look for any existing data
            if (context.UserFunds.Any())
            {
                return;   // DB has been seeded
            }

          
            // Add dummy data for MutualFunds
            var mutualFunds = new MutualFund[]
          {
         new MutualFund
         {
             FundName = "Mid-Cap Equity Fund",
             FundType = "Mid-Cap",
             RiskCategory = "High",
             AssetClass = "Equity",
             Nav = 150.00M,
             ExpenseRatio = 1.2M,
             Aum = 5000000M,
             ReturnPercentage = 10.5M,
             MinimumInvestAmount = 1000M,
             ExitLoad = 1.0M,
             ExitLoadPeriod = 3
         },
         new MutualFund
         {
             FundName = "High-Cap Debt Fund",
             FundType = "Large-Cap",
             RiskCategory = "Low",
             AssetClass = "Debt",
             Nav = 105.50M,
             ExpenseRatio = 0.5M,
             Aum = 3000000M,
             ReturnPercentage = 5.0M,
             MinimumInvestAmount = 2000M,
             ExitLoad = 0.5M,
             ExitLoadPeriod = 6
         },
         new MutualFund
         {
             FundName = "Balanced Fund",
             FundType = "Large-Cap",
             RiskCategory = "Medium",
             AssetClass = "Balanced",
             Nav = 120.00M,
             ExpenseRatio = 1.0M,
             Aum = 7000000M,
             ReturnPercentage = 8.0M,
             MinimumInvestAmount = 1500M,
             ExitLoad = 1.0M,
             ExitLoadPeriod = 12
         },
         new MutualFund
         {
             FundName = "Low-Cap Equity Fund",
             FundType = "Small-Cap",
             RiskCategory = "High",
             AssetClass = "Equity",
             Nav = 95.00M,
             ExpenseRatio = 1.8M,
             Aum = 2000000M,
             ReturnPercentage = 12.5M,
             MinimumInvestAmount = 1000M,
             ExitLoad = 1.5M,
             ExitLoadPeriod = 1
         },
         new MutualFund
         {
             FundName = "Global Debt Fund",
             FundType = "Large-Cap",
             RiskCategory = "Medium",
             AssetClass = "Debt",
             Nav = 110.00M,
             ExpenseRatio = 1.5M,
             Aum = 8000000M,
             ReturnPercentage = 7.0M,
             MinimumInvestAmount = 3000M,
             ExitLoad = 2.0M,
             ExitLoadPeriod = 9
         },
         new MutualFund
         {
             FundName = "High-Cap Balanced Fund",
             FundType = "Large-Cap",
             RiskCategory = "Medium",
             AssetClass = "Balanced",
             Nav = 140.00M,
             ExpenseRatio = 1.1M,
             Aum = 6000000M,
             ReturnPercentage = 9.5M,
             MinimumInvestAmount = 2500M,
             ExitLoad = 1.0M,
             ExitLoadPeriod = 2
         },
         new MutualFund
         {
             FundName = "Low-Cap Debt Fund",
             FundType = "Small-Cap",
             RiskCategory = "Low",
             AssetClass = "Debt",
             Nav = 85.00M,
             ExpenseRatio = 0.8M,
             Aum = 4000000M,
             ReturnPercentage = 4.5M,
             MinimumInvestAmount = 2000M,
             ExitLoad = 0.5M,
             ExitLoadPeriod = 4
         },
         new MutualFund
         {
             FundName = "Mid-Cap Balanced Fund",
             FundType = "Mid-Cap",
             RiskCategory = "High",
             AssetClass = "Balanced",
             Nav = 130.00M,
             ExpenseRatio = 1.7M,
             Aum = 9000000M,
             ReturnPercentage = 11.0M,
             MinimumInvestAmount = 2000M,
             ExitLoad = 1.2M,
             ExitLoadPeriod = 5
         },
         new MutualFund
         {
             FundName = "Global Equity Fund",
             FundType = "Large-Cap",
             RiskCategory = "High",
             AssetClass = "Equity",
             Nav = 190.00M,
             ExpenseRatio = 2.2M,
             Aum = 11000000M,
             ReturnPercentage = 16.0M,
             MinimumInvestAmount = 4000M,
             ExitLoad = 2.5M,
             ExitLoadPeriod = 8
         },
         new MutualFund
         {
             FundName = "Mid-Cap Debt Fund",
             FundType = "Mid-Cap",
             RiskCategory = "Medium",
             AssetClass = "Debt",
             Nav = 115.00M,
             ExpenseRatio = 1.0M,
             Aum = 5500000M,
             ReturnPercentage = 6.5M,
             MinimumInvestAmount = 1500M,
             ExitLoad = 1.0M,
             ExitLoadPeriod = 6
         }
          };

            foreach (MutualFund fund in mutualFunds)
            {
                context.MutualFunds.Add(fund);
            }
            context.SaveChanges();






        }
    }
}
