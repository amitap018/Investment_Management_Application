using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class MutualFund
    {
        [Key]
        public long FundId { get; set; }
        public string FundName { get; set; }
        public string FundType { get; set; }
        public string RiskCategory { get; set; }
        public string AssetClass { get; set; }
        public decimal Nav { get; set; }
        public decimal ExpenseRatio { get; set; }
        public decimal Aum { get; set; }
        public decimal ReturnPercentage { get; set; }
        public decimal MinimumInvestAmount { get; set; }
        public decimal ExitLoad { get; set; }
        public int ExitLoadPeriod { get; set; }


        public ICollection<UserFund> UserFunds { get; set; } = new List<UserFund>();
        public ICollection<SIP> SIPs { get; set; } = new List<SIP>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
