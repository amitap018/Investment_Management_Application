using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class UserFund
    {
        [Key]
        public long FolioNumber { get; set; }


        public long FundId { get; set; }
        public decimal OneDayChange { get; set; }
        public decimal InvestedAmount { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal PAndL { get; set; }
        public decimal ExpenseAmount { get; set; }
        public decimal Taxes { get; set; }
        public decimal Commission { get; set; }
        public decimal Units { get; set; }

        public Status Status { get; set; } = Status.Active;



        public long UserId { get; set; }

        [ForeignKey("FundId")]
        public MutualFund MutualFund { get; set; }

        [ForeignKey("UserId")]
        public UserDetails UserDetails { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public enum Status
    {
        Active,
        Inactive
    }
}
