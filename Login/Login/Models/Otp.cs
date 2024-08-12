using System;
using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class Otp
    {
        [Key]
        public long OtpId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }

        public DateTime ExpirationTime { get; set; }
    }
}
