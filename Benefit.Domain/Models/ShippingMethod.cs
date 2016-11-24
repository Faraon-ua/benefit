using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Benefit.Domain.Models
{
    public class ShippingMethod
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }
        public int? FreeStartsFrom { get; set; }
        public int? CostBeforeFree { get; set; }
        public int RegionId { get; set; }
        [JsonIgnore]
        public Region Region { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        [JsonIgnore]
        public Seller Seller { get; set; }
    }
}
