using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Login.Models
{
    public class UserDetails
    {
        [Key]
        public long UserId { get; set; }

        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Account Number should be a positive number.")]
        public string AccountNumber { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone Number must be a 10 digit positive number.")]
        public string PhoneNumber { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "PAN must be in the format of 5 alphabets, 4 numbers, and 1 alphabet.")]
        public string Pan { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "Aadhaar Number must be a 12 digit positive number.")]
        public string AadhaarNumber { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "IFSC should not contain negative numbers or symbols.")]
        public string IfscCode { get; set; }

        [Required]
        public string BankName { get; set; }

        public string? DematId { get; set; } // Make this field optional

        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

        public virtual ICollection<Grievance> Grievances { get; set; } = new List<Grievance>();

        public ICollection<UserFund> UserFunds { get; set; } = new List<UserFund>();

        public ICollection<SIP> SIPs { get; set; } = new List<SIP>();

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
