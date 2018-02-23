using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class ProductPartialViewModel
    {
        public Product Product { get; set; }
        public KeyValuePair<bool, string> AvailableForPurchase { get; set; }
        public string CategoryUrl { get; set; }
        public string SellerUrl { get; set; }
        public string PageMark { get; set; }
    }
}
