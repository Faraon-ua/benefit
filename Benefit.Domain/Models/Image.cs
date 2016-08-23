using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum ImageType
    {
        Cover,
        Gallery
    }
    public class Image
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        public string CategoryId { get; set; }
        public string ProductId { get; set; }
        public string SellerId { get; set; }
    }
}
