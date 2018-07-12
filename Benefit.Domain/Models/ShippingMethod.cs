using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Benefit.Domain.Models
{
    public class ShippingMethodComparer: IEqualityComparer<ShippingMethod>
    {
        public bool Equals(ShippingMethod x, ShippingMethod y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(ShippingMethod obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    public class ShippingMethod
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }
        public int? FreeStartsFrom { get; set; }
        public int? CostBeforeFree { get; set; }
        public bool SkipOrderAddress { get; set; }
        [Required(ErrorMessage = "Регіон доставки не обрано")]
        public int? RegionId { get; set; }
        [JsonIgnore]
        public Region Region { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        [JsonIgnore]
        public Seller Seller { get; set; }
    }
}
