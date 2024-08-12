using Login.Models;
using System.ComponentModel.DataAnnotations;

namespace Login.ViewModels
{
    public class MutualFundOrderViewModel
    {
        public MutualFund? MutualFund { get; set; }

        [Required(ErrorMessage = "Order Type is required.")]
        public string OrderType { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal? Amount { get; set; }

        public string? Frequency { get; set; } // Assuming this is not required for Lump Sum

        public DateTime? SipDate { get; set; }
    }
}