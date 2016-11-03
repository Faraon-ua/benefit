using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class ProductPartialViewModel
    {
        public Product Product { get; set; }
        public string CategoryUrl { get; set; }
        public string SellerUrl { get; set; }
    }
}
