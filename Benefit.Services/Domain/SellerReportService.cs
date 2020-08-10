using Benefit.Domain.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using Benefit.Domain.Models;
using Benefit.Common.Helpers;
using Benefit.Services.Files;
using System.Text.RegularExpressions;

namespace Benefit.Services.Domain
{
    public class SellerReportService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public byte[] GenerateSellerReport(string sellerId, DateTime startDate, DateTime endDate)
        {
            var seller = db.Sellers.Find(sellerId);
            var result = new List<Report>();
            var orderIds = db.Orders
                .Include(entry => entry.OrderProducts)
                .Where(entry => entry.Status == OrderStatus.Finished && entry.SellerId == sellerId && entry.Time > startDate && entry.Time < endDate)
                .Select(entry => entry.Id).ToList();
            var orderProducts = db.OrderProducts.Include(entry => entry.Order).Where(entry => orderIds.Contains(entry.OrderId)).ToList();
            var categories = orderProducts.Select(entry => entry.CategoryName).Distinct().ToList();
            var orderNumbers = orderProducts.Select(entry => entry.Order.OrderNumber).Distinct().ToList();
            var sellerTransactions = db.SellerTransactions.Where(entry => orderNumbers.Contains(entry.OrderNumber) && entry.Writeoff != null);

            foreach (var category in categories)
            {
                var categoryResult = new List<Report>();
                var products = orderProducts.Where(entry => entry.CategoryName == category).OrderBy(entry => entry.Order.Time).ToList();
                foreach (var product in products)
                {
                    if (product.Amount <= 0) continue;
                    var sellerTransaction = sellerTransactions.FirstOrDefault(entry => entry.ProductSKU == product.ProductSku && entry.OrderNumber == product.Order.OrderNumber) ??
                        new SellerTransaction
                        {
                            Writeoff = product.ProductPrice * product.Amount * seller.TotalDiscount / 100
                        };
                    var report = new Report()
                    {
                        Date = product.Order.Time.ToString("yyyy-MM-dd"),
                        OrderId = product.Order.OrderNumber.ToString(),
                        OrderStatus = Enumerations.GetDisplayNameValue(product.Order.Status),
                        ProductSKU = product.ProductSku.ToString(),
                        Category = category == null ? "Інше" : category.Replace(",", " "),
                        Price = product.ProductPrice,
                        Amount = product.Amount,
                        Sum = product.ProductPrice * product.Amount,
                        Percent = sellerTransaction.FeePercent == 0 ? seller.TotalDiscount : sellerTransaction.FeePercent,
                        Charge = sellerTransaction.Writeoff.Value,
                        Bonuses = product.Order.PaymentType == PaymentType.Bonuses ? product.ProductPrice * product.Amount : 0,
                        ProductName = product.ProductName.Replace(",", " "),
                        Shipment = Regex.Replace(string.Format("{0} {1}", product.Order.ShippingName, product.Order.ShippingAddress).Replace(",", " ").Replace("\r", string.Empty).Replace("\t", string.Empty), " {2,}", " "),
                        ShipmentPrice = product.Order.ShippingCost,
                        Customer = product.Order.UserName,
                        Phone = product.Order.UserPhone,
                        TTN = product.Order.ShippingTrackingNumber
                    };
                    categoryResult.Add(report);
                }
                var sumReport = new Report()
                {
                    Category = category == null ? "Інше" + " Всього" : category.Replace(",", " ") + " Всього",
                    Amount = categoryResult.Sum(entry => entry.Amount),
                    Sum = categoryResult.Sum(entry => entry.Sum),
                    Charge = categoryResult.Sum(entry => entry.Charge),
                    Bonuses = categoryResult.Sum(entry => entry.Bonuses)
                };
                result.AddRange(categoryResult);
                result.Add(sumReport);
            }
            var resultReport = new Report()
            {
                Category = "Всього",
                Amount = result.Where(entry => entry.OrderId != null).Sum(entry => entry.Amount),
                Sum = result.Where(entry => entry.OrderId != null).Sum(entry => entry.Sum),
                Charge = result.Where(entry => entry.OrderId != null).Sum(entry => entry.Charge),
                Bonuses = result.Where(entry => entry.OrderId != null).Sum(entry => entry.Bonuses)
            };
            result.Add(resultReport);
            var fileService = new FilesExportService();
            return fileService.CreateCSVFromGenericList(result, "\t");
        }
    }
}
