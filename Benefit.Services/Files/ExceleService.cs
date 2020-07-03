using Benefit.Domain.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benefit.Services.Files
{
    public class ExceleService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public void GenerateSellerReport(string sellerId, int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = new DateTime(year, month, 31, 23, 59, 59);
            var orderProducts = db.Orders.Include(entry=>entry.OrderProducts)
                .Where(entry => entry.SellerId == sellerId && entry.Time > startDate && entry.Time < endDate)
                .SelectMany(entry=>entry.OrderProducts)
                .ToList();
            var productIds = orderProducts.Select(entry => entry.ProductId).ToList();
            var products = db.Products.Include(entry => entry.Category.MappedParentCategory)
                .Where(entry => productIds.Contains(entry.Id)).ToList();
        }
    }
}
