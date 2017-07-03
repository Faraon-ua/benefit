using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum NotificationChannelType
    {
        Phone,
        Email,
        Facebook
    }
    public class NotificationChannel
    {
        public string Id { get; set; }
        [MaxLength(128)]
        public string Address { get; set; }
        public NotificationChannelType ChannelType { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
