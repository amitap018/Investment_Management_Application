using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Login.Models
{
    public class Feedback
    {
        [Key]
        public long FeedbackId { get; set; }

        [Required]
        public int Rating { get; set; }

        [Required]
        public string Description { get; set; }

        [ForeignKey("UserDetails")]
        public long UserId { get; set; }

        public virtual UserDetails? UserDetails { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
