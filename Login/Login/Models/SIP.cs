using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class SIP
    {
        [Key]
        public long SipId { get; set; }
        public long FundId { get; set; }
        public decimal SipAmount { get; set; }
        public string Frequency { get; set; }
        public DateTime InstallmentDate { get; set; }
        public DateTime StartDate { get; set; }
        public string Status { get; set; }
        public long FolioNumber { get; set; }
        public long UserId { get; set; }

        [ForeignKey("FundId")]
        public MutualFund MutualFund { get; set; }

        [ForeignKey("FolioNumber")]
        public UserFund UserFund { get; set; }

        [ForeignKey("UserId")]
        public UserDetails UserDetails { get; set; }
    }
}
