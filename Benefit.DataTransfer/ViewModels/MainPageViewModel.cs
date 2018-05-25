using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class MainPageViewModel
    {
        public List<ProductPartialViewModel> FeaturedProducts { get; set; }
        public List<ProductPartialViewModel> NewProducts { get; set; }
        public InfoPage Description { get; set; } 
        public List<InfoPage> News { get; set; } 
        public List<Banner> MobileBanners { get; set; } 
        public List<Banner> PrimaryBanners { get; set; } 
        public List<Banner> TopSideBanners { get; set; } 
        public List<Banner> BottomSideBanners { get; set; } 
        public Banner FirstRowBanner { get; set; } 
        public Banner SecondRowBanner { get; set; } 
        public List<Seller> Brands { get; set; }
    }
}
