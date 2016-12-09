using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.Services.Domain
{
    public class OrderService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public List<Order> GetOrders(string sellerId, int skip, int take = ListConstants.DefaultTakePerPage)
        {
            var orders = db.Orders as IQueryable<Order>;
            if (sellerId != null)
            {
                orders = orders.Where(entry => entry.SellerId == sellerId);
            }
            orders = orders.OrderByDescending(entry => entry.Time);
            return orders.Skip(skip).Take(take).ToList();
        } 

        public string AddOrder(CompleteOrderViewModel model)
        {
            var seller = db.Sellers.FirstOrDefault(entry => entry.Id == model.Order.SellerId);
            var order = model.Order;
            order.Id = Guid.NewGuid().ToString();
            var orderNumber = db.Orders.Max(entry => (int?)entry.OrderNumber) ?? SettingsService.OrderMinValue;
            order.OrderNumber = orderNumber + 1;

            var orderSum =
                order.OrderProducts.Sum(
                    entry =>
                        entry.ProductPrice * entry.Amount +
                        entry.OrderProductOptions.Sum(option => option.ProductOptionPriceGrowth * option.Amount));
            order.Sum = orderSum;
            order.Description = model.Comment;
            order.PersonalBonusesSum = order.Sum * seller.UserDiscount / 100;
            order.PointsSum = order.Sum / SettingsService.DiscountPercentToPointRatio[seller.TotalDiscount];
            order.SellerName = seller.Name;

            var shipping = db.ShippingMethods.FirstOrDefault(entry => entry.Id == model.ShippingMethodId);
            order.ShippingCost = (double)(orderSum < shipping.FreeStartsFrom ? shipping.CostBeforeFree : 0);
            order.ShippingName = shipping.Name;

            var address = db.Addresses.FirstOrDefault(entry => entry.Id == model.AddressId);
            order.ShippingAddress = string.Format("{0}; {1}; {2}, {3}", address.FullName, address.Phone, address.Region.Name_ua, address.AddressLine);

            order.Time = DateTime.UtcNow;
            order.OrderType = OrderType.BenefitSite;
            order.PaymentType = model.PaymentType.Value;

            //add order to DB
            db.Orders.Add(order);
            foreach (var product in order.OrderProducts)
            {
                product.OrderId = order.Id;
                db.OrderProducts.Add(product);
                foreach (var orderProductOption in product.OrderProductOptions)
                {
                    orderProductOption.OrderId = order.Id;
                    orderProductOption.ProductId = product.ProductId;
                    db.OrderProductOptions.Add(orderProductOption);
                }
            }

            db.SaveChanges();
            Cart.Cart.CurrentInstance.Clear();
            var cartMumberCookie = HttpContext.Current.Response.Cookies["cartNumber"];
            if (cartMumberCookie != null)
            {
                cartMumberCookie.Expires = DateTime.UtcNow.AddDays(-1);
            }
            return order.OrderNumber.ToString();
        }

        public void DeleteOrder(string orderId)
        {
            var order = db.Orders.Include(entry => entry.OrderProducts).Include(entry => entry.OrderProductOptions).FirstOrDefault(entry => entry.Id == orderId);
            db.OrderProductOptions.RemoveRange(order.OrderProductOptions);
            db.OrderProducts.RemoveRange(order.OrderProducts);
            db.Transactions.RemoveRange(db.Transactions.Where(entry => entry.OrderId == orderId));
            db.Orders.Remove(order);
            db.SaveChanges();
        }
    }
}
