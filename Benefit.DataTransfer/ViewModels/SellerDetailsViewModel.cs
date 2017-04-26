using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class SellerDetailsViewModel
    {
        public Seller Seller { get; set; }
        public string Specification { get; set; }
        public BreadCrumbsViewModel Breadcrumbs { get; set; }
        public bool CanReview { get; set; } 
    }
}
