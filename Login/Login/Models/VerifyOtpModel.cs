using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class VerifyOtpModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
