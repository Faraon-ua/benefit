using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Benefit.Domain.DataAccess;

namespace Benefit.Domain.Models
{
    public class OrderProduct
    {
        public OrderProduct()
        {
            OrderProductOptions = new Collection<OrderProductOption>();
        }
        [Key, Column(Order = 0)]
        [MaxLength(128)]
        public string OrderId { get; set; }
        public virtual Order Order { get; set; }
        [Key, Column(Order = 1)]
        [MaxLength(128)]
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public double ProductPrice { get; set; }
        public double Amount { get; set; }
        [NotMapped]
        public bool IsWeightProduct { get; set; }
        [NotMapped]
        public ICollection<OrderProductOption> OrderProductOptions { get; set; }
        [NotMapped]
        public ICollection<OrderProductOption> DbOrderProductOptions
        {
            get
            {
                using (var db = new ApplicationDbContext())
                {
                    return db.OrderProductOptions.Where(entr => entr.ProductId == ProductId).ToList();
                }
            }
        }
    }
}
