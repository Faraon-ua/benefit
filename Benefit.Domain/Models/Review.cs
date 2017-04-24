using System;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class Review
    {
        public string Id { get; set; }
        [MaxLength(512)]
        [Required(ErrorMessage = "Текст відгуку обов'язковий для заповнення")]
        public string Message { get; set; }
        [MaxLength(64)]
        public string UserFullName { get; set; }
        public DateTime Stamp { get; set; }
        [Required(ErrorMessage = "Рейтинг не вказано")]
        public int Rating { get; set; }
        public bool IsActive { get; set; }
        [MaxLength(128)]
        public string ProductId { get; set; }
        public Product Product { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
    }
}
