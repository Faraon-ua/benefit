﻿using Benefit.Domain.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using Benefit.Domain.Models;
using Benefit.Common.Helpers;
using Benefit.Services.Files;

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
                .Where(entry => entry.SellerId == sellerId && entry.Time > startDate && entry.Time < endDate).Select(entry=>entry.Id).ToList();
            var orderProducts = db.OrderProducts.Include(entry=>entry.Order).Where(entry=>orderIds.Contains(entry.OrderId)).ToList();
            var categories = orderProducts.Select(entry => entry.CategoryName).Distinct().ToList();
            var orderNumbers = orderProducts.Select(entry => entry.Order.OrderNumber).Distinct().ToList();
            var sellerTransactions = db.SellerTransactions.Where(entry => orderNumbers.Contains(entry.OrderNumber) && entry.Writeoff != null);

            foreach (var category in categories)
            {
                var categoryResult = new List<Report>();
                var products = orderProducts.Where(entry => entry.CategoryName == category).OrderBy(entry => entry.Order.Time).ToList();
                foreach (var product in products)
                {
                    var sellerTransaction = sellerTransactions.FirstOrDefault(entry => entry.ProductSKU == product.ProductSku && entry.OrderNumber == product.Order.OrderNumber);
                    if (sellerTransaction != null)
                    {
                        var report = new Report()
                        {
                            //Id = entry.Order.OrderNumber.ToString(),
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
                            ProductName = product.ProductName.Replace(",", " "),
                            Shipment = string.Format("{0} {1}", product.Order.ShippingName, product.Order.ShippingAddress).Replace(","," "),
                            ShipmentPrice = product.Order.ShippingCost,
                            Customer = product.Order.UserName,
                            Phone = product.Order.UserPhone,
                            TTN = product.Order.ShippingTrackingNumber
                        };
                        categoryResult.Add(report);
                    }
                }
                var sumReport = new Report()
                {
                    Category = category == null ? "Інше" + " Всього" : category.Replace(",", " ") + " Всього",
                    Amount = categoryResult.Sum(entry=>entry.Amount),
                    Sum = categoryResult.Sum(entry=>entry.Sum),
                    Charge = categoryResult.Sum(entry => entry.Charge)
                };
                result.AddRange(categoryResult);
                result.Add(sumReport);
            }
            var resultReport = new Report()
            {
                Category = "Всього",
                Amount = result.Where(entry=>entry.OrderId!=null).Sum(entry => entry.Amount),
                Sum = result.Where(entry => entry.OrderId != null).Sum(entry => entry.Sum),
                Charge = result.Where(entry => entry.OrderId != null).Sum(entry => entry.Charge)
            };
            result.Add(resultReport);
            var fileService = new FilesExportService();
            return fileService.CreateCSVFromGenericList(result, ",");
        }
    }
}
