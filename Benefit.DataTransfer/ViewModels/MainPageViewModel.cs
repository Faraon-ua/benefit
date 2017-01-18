using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class MainPageViewModel
    {
        public List<Product> BestSellers { get; set; }
        public List<Product> NewProducts { get; set; }
        public List<Banner> Banners { get; set; } 
    }
}
