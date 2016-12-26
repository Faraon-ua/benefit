using System;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class Message
    {
        public string Id { get; set; }
        [Required]
        [MaxLength(250)]
        public string Text { get; set; }
        public DateTime TimeStamp { get; set; }
        [MaxLength(128)]
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
