using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class ReviewsViewModel
    {
        public string TargetName { get; set; }
        public bool CanReview { get; set; }
        public int AvarageRating { get; set; }
        public string ProductId { get; set; }
        public string SellerId { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
