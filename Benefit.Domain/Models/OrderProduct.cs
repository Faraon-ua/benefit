using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Benefit.Domain.DataAccess;

namespace Benefit.Domain.Models
{
    [Serializable]
    public class OrderProduct
    {
        public OrderProduct()
        {
            OrderProductOptions = new Collection<OrderProductOption>();
        }
        public string Id { get; set; }
        [MaxLength(128)]
        public string OrderId { get; set; }
        [NonSerialized]
        public Order Order;
        [MaxLength(128)]
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int? ProductSku { get; set; }
        [NotMapped]
        public string SellerId { get; set; }
        [NotMapped]
        public string ProductUrlName { get; set; }
        [NotMapped]
        public string ProductImageUrl { get; set; }
        public double ProductPrice { get; set; }
        [NotMapped]
        public double? WholesaleProductPrice { get; set; }
        [NotMapped]
        public double PriceGrowth { get; set; }
        [NotMapped]
        public double? WholesaleFrom { get; set; }
        public double Amount { get; set; }
        public int Index { get; set; }
        [NotMapped]
        public int? AvailableAmount { get; set; }
        [NotMapped]
        public bool IsWeightProduct { get; set; }
        [NotMapped]
        public double BonusesAcquired { get; set; }
        [NotMapped]
        public string NameSuffix{ get; set; }

        public ICollection<OrderProductOption> OrderProductOptions { get; set; }

        [NotMapped]
        public double ActualPrice
        {
            get
            {
                if (WholesaleFrom.HasValue && WholesaleProductPrice.HasValue && Amount >= WholesaleFrom.Value)
                {
                    return WholesaleProductPrice.Value;
                }
                return ProductPrice;
            }
        }

        [NotMapped]
        public ICollection<OrderProductOption> DbOrderProductOptions
        {
            get
            {
                using (var db = new ApplicationDbContext())
                {
                    return db.OrderProductOptions.Where(entr => entr.OrderProductId == Id).ToList();
                }
            }
        }
    }
}
