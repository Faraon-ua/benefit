using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class ReviewsViewModel
    {
        public string TargetName { get; set; }
        public bool ApplyMicrodata { get; set; }
        public bool CanReview { get; set; }
        public string ProductId { get; set; }
        public string SellerId { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }

    public class ReviewStarsViewModel
    {
        public bool SmallStars { get; set; }
        public bool IsActive { get; set; }
        public int? Rating { get; set; }
    }
}
