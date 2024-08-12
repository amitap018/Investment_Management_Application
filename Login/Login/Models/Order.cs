using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class Order
    {
        [Key]
        public long OrderId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Units { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public long FundId { get; set; }
        public long UserId { get; set; }

        public Boolean IsSip { get; set; }

        public string TransactionType { get; set; }
        public long FolioNumber { get; set; }

        [ForeignKey("FundId")]
        public MutualFund MutualFund { get; set; }

        [ForeignKey("UserId")]
        public UserDetails UserDetails { get; set; }

        [ForeignKey("FolioNumber")]
        public UserFund UserFund { get; set; }
    }
}
