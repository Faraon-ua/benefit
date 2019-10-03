using System;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum SellerTransactionType
    {
        [Display(Name = "Резервування суми по зробленому замовленню")]
        Reserve,
        [Display(Name = "Комісія за продаж")]
        SalesComission,
        [Display(Name = "Зняття резерву за невиконане замовлення")]
        FailOrderReserveReturn,
        [Display(Name = "Поповнення рахунку")]
        Refill
    }
    public class SellerTransaction
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public DateTime Time { get; set; }
        public SellerTransactionType Type { get; set; }
        public int OrderNumber { get; set; }
        public int ProductSKU { get; set; }
        public string ProductUrlName { get; set; }
        public double Price { get; set; }
        public double TotalPrice { get; set; }
        public double Amount { get; set; }
        public double? Charge { get; set; }
        public double? Writeoff { get; set; }
        public double Balance { get; set; }
        public double GreyZoneBalance { get; set; }
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
    }
}
