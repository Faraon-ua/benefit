using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benefit.Domain.Models
{
    public class SellerReport
    {
        public string Id { get; set; }
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(128)]
        public string FileUrl { get; set; }
    }
    public class Report
    {
        //[DisplayName("ID операції")]
        //public string Id { get; set; }
        [DisplayName("Дата операції")]
        public string Date { get; set; }
        [DisplayName("ID замовлення")]
        public string OrderId { get; set; }
        [DisplayName("Статус замовлення")]
        public string OrderStatus{ get; set; }
        [DisplayName("ID товару")]
        public string ProductSKU { get; set; }
        [DisplayName("Категорія товару")]
        public string Category { get; set; }
        [DisplayName("Ціна товару (грн)")]
        public double? Price { get; set; }
        [DisplayName("К-сть товарів")]
        public double Amount { get; set; }
        [DisplayName("Сума загалом (грн)")]
        public double Sum { get; set; }
        [DisplayName("Ставка %")]
        public double? Percent { get; set; }
        [DisplayName("Списано (грн)")]
        public double Charge { get; set; }
        [DisplayName("Назва товару")]
        public string ProductName { get; set; }
        [DisplayName("Доставка")]
        public string Shipment { get; set; }
        [DisplayName("Вартість доставки")]
        public double? ShipmentPrice { get; set; }
        [DisplayName("Покупець")]
        public string Customer { get; set; }
        [DisplayName("Телефон")]
        public string Phone { get; set; }
        [DisplayName("ТТН")]
        public string TTN { get; set; }
    }
}
