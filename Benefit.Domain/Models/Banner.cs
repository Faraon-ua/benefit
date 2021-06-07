using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum BannerType
    {
        [Display(Name = "Головна сторінка - основний")]
        PrimaryMainPage,
        [Display(Name = "Особистий кабінет")]
        PartnerPageBanners,
        [Display(Name = "Додатковий (верхній)")]
        SideTopMainPage,
        [Display(Name = "Додатковий (нижній)")]
        SideBottomMainPage,
        [Display(Name = "Головна сторінка - перша полоса")]
        FirstRowMainPage,
        [Display(Name = "Головна сторінка - друга полоса")]
        SecondRowMainPage,
        [Display(Name = "Мобільна версія")]
        MobileMainPage
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
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
    }
}
