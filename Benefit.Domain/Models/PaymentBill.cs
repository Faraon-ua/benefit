using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum BillType
    {
        [Description("BCUA")]
        [Display(Name = "Роялті")]
        Royalty
    }
    public enum BillStatus
    {
        [Display(Name = "Очікується оплата", Description = "text-danger")]
        AwaitingPayment,
        [Display(Name = "Оплачено", Description = "text-success")]
        Paid
    }
    public class PaymentBill
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        public string Number { get; set; }
        [Required]
        public int InnerNumber { get; set; }
        public DateTime Time { get; set; }
        public BillType Type { get; set; }
        public double Sum { get; set; }
        public BillStatus Status { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
    }
}
