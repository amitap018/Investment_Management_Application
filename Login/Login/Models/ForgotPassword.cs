using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
