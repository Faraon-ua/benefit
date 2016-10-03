using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum ImageType
    {
        SellerLogo,
        SellerGallery,
        ProductGallery,
        NewsLogo
    }
    public class Image
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        [MaxLength(256)]
        public string ImageUrl { get; set; }
        public ImageType ImageType { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        [MaxLength(128)]
        public string ProductId { get; set; }
        public Product Product { get; set; }
    }
}
