using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class Favorite
    {
        [MaxLength(128)]
        [Key, Column(Order = 0)]
        public string UserId { get; set; }

        [MaxLength(128)]
        [Key, Column(Order = 1)]
        public string ProductId { get; set; }
        
        public Product Product { get; set; }
        public ApplicationUser User { get; set; }
    }
}
