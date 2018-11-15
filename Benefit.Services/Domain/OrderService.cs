using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using Benefit.Common.Constants;
using Benefit.Common.Extensions;
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
            //handle wholesale prices
            foreach (var orderProduct in order.OrderProducts)
            {
                orderProduct.ProductPrice = orderProduct.ActualPrice;
            }

            order.Sum = order.GetOrderSum();
            order.Description =
                string.Format("{0}<br/>--------------------------------------------<br/> Заказ створено на {1}",
                    model.Comment, HttpContext.Current.Request.Url.Host);
            //order.PersonalBonusesSum = order.SumWithDiscount * seller.UserDiscount / 100;
            order.PersonalBonusesSum = order.OrderProducts.Sum(entry=>entry.BonusesAcquired);

            order.PointsSum = Double.IsInfinity(order.Sum / SettingsService.DiscountPercentToPointRatio[seller.TotalDiscount]) ? 0 : order.SumWithDiscount / SettingsService.DiscountPercentToPointRatio[seller.TotalDiscount];
            order.SellerName = seller.Name;

            var shipping = db.ShippingMethods.FirstOrDefault(entry => entry.Id == model.ShippingMethodId);
            order.ShippingCost = order.Sum < shipping.FreeStartsFrom ? (shipping.CostBeforeFree ?? default(double)) : 0;
            order.ShippingName = shipping.Name;
            var address = db.Addresses.FirstOrDefault(entry => entry.Id == model.AddressId);
            if (address != null)
            {
                order.ShippingAddress = string.Format("{0}; {1}; {2}, {3}", address.FullName, address.Phone,
                    address.Region.Name_ua, address.AddressLine);
            }

            order.Time = DateTime.UtcNow;
            order.OrderType = OrderType.BenefitSite;
            order.PaymentType = model.PaymentType.Value;

            //add order to DB
            db.Orders.Add(order);
            var i = 0;
            foreach (var product in order.OrderProducts)
            {
                product.OrderId = order.Id;
                product.Index = i++;
                db.OrderProducts.Add(product);
                foreach (var orderProductOption in product.OrderProductOptions)
                {
                    orderProductOption.OrderId = order.Id;
                    orderProductOption.ProductId = product.ProductId;
                    db.OrderProductOptions.Add(orderProductOption);
                }
            }
            db.SaveChanges();

            //add transaction to reduce bonuses account
            if (order.PaymentType == PaymentType.Bonuses)
            {
                var TransactionsService = new TransactionsService();
                TransactionsService.AddBonusesOrderTransaction(order);
            }

            Cart.Cart.CurrentInstance.ClearSellerOrder(model.Order.SellerId);
            var cartNumberCookie = HttpContext.Current.Response.Cookies["cartNumber"];
            if (cartNumberCookie != null)
                if (Cart.Cart.CurrentInstance.Orders.Count == 0)
                {
                    cartNumberCookie.Expires = DateTime.UtcNow.AddDays(-1);
                }
                else
                {
                    cartNumberCookie.Value = Cart.Cart.CurrentInstance.GetOrderProductsCountAndPrice().ProductsNumber.ToString();
                }
            return order.OrderNumber.ToString();
        }

        public void DeleteOrder(string orderId)
        {
            var order = db.Orders.Include(entry => entry.OrderProducts).Include(entry => entry.User).Include(entry => entry.Transactions).Include(entry => entry.OrderProductOptions).FirstOrDefault(entry => entry.Id == orderId);
            var now = DateTime.UtcNow;
            var seller = db.Sellers.FirstOrDefault(entry => entry.Id == order.SellerId);
            if (seller != null)
            {
                if (order.Time > now.StartOfMonth() && order.Time < now.EndOfMonth())
                {
                    seller.PointsAccount = seller.PointsAccount - order.PointsSum;
                }
                if (order.Time > now.StartOfMonth().AddMonths(-1) && order.Time < now.EndOfMonth().AddMonths(-1))
                {
                    seller.HangingPointsAccount = seller.HangingPointsAccount - order.PointsSum;
                }
                db.Entry(seller).State = EntityState.Modified;
            }
            if (order.Time > now.StartOfMonth() && order.Time < now.EndOfMonth())
            {
                order.User.PointsAccount = order.User.PointsAccount - order.PointsSum;
                order.User.CurrentBonusAccount = order.User.CurrentBonusAccount - order.PersonalBonusesSum;
                if (order.PaymentType == PaymentType.Bonuses)
                {
                    order.User.BonusAccount = order.User.BonusAccount + order.Sum;
                }
            }
            if (order.Time > now.StartOfMonth().AddMonths(-1) && order.Time < now.EndOfMonth().AddMonths(-1))
            {
                order.User.HangingPointsAccount = order.User.HangingPointsAccount - order.PointsSum;
                order.User.HangingBonusAccount = order.User.HangingBonusAccount - order.PersonalBonusesSum;
                if (order.PaymentType == PaymentType.Bonuses)
                {
                    order.User.BonusAccount = order.User.BonusAccount + order.Sum;
                }
            }

            db.Transactions.RemoveRange(order.Transactions);
            db.OrderProductOptions.RemoveRange(order.OrderProductOptions);
            db.OrderProducts.RemoveRange(order.OrderProducts);
            db.Entry(order.User).State = EntityState.Modified;
            db.Orders.Remove(order);
            db.SaveChanges();
        }
    }
}
