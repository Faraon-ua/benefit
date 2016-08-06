namespace Benefit.Domain.Models
{
    public class ShippingMethod
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Region { get; set; }
        public int? FreeStartsFrom { get; set; }
        public int CostBeforeFree { get; set; }
    }
}
