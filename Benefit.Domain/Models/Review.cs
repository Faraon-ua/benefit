using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Benefit.Domain.Models
{
    public class Review
    {
        public Review()
        {
            ChildReviews = new Collection<Review>();
        }
        public string Id { get; set; }
        [MaxLength(512, ErrorMessage = "Текст відгуку не може перевищувати 512 символів")]
        [Required(ErrorMessage = "Текст відгуку обов'язковий для заповнення")]
        public string Message { get; set; }
        [MaxLength(64)]
        public string UserFullName { get; set; }
        public DateTime Stamp { get; set; }
        public int? Rating { get; set; }
        public bool IsActive { get; set; }
        [MaxLength(128)]
        public string ReviewId { get; set; }
        public Review ParentReview { get; set; }
        [MaxLength(128)]
        public string ProductId { get; set; }
        public Product Product { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        public ICollection<Review> ChildReviews { get; set; }

        public Review Answer
        {
            get { return ChildReviews.FirstOrDefault(); }
        }
    }
}
