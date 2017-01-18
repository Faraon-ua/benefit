using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum BannerType
    {
        MainPageBanners
    }
    public class Banner
    {
        public string Id { get; set; }
        [Required]
        [MaxLength(256)]
        public string ImageUrl { get; set; }
        [Required]
        [MaxLength(256)]
        public string NavigationUrl { get; set; }
        public BannerType BannerType { get; set; }
        public int Order { get; set; }
    }
}
