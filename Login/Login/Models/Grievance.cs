using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Login.Models
{
    public class Grievance
    {
        [Key]
        public long GrievanceId { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Description { get; set; }

        [ForeignKey("UserDetails")]
        public long UserId { get; set; }

        public virtual UserDetails? UserDetails { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
