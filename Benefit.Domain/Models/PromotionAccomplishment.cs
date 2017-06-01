using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class PromotionAccomplishment
    {
        [Key, Column(Order = 0)]
        public string PromotionId { get; set; }
        [Key, Column(Order = 1)]
        public string UserId { get; set; }
        public int AccomplishmentsNumber { get; set; }
        public Promotion Promotion { get; set; }
        public ApplicationUser User { get; set; }
    }
}
