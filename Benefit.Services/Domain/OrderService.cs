using System;
using System.Linq;
using System.Web;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.Services.Domain
{
    public class OrderService
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public void AddOrder(CompleteOrderViewModel model)
        {
            var seller = db.Sellers.FirstOrDefault(entry => entry.Id == model.Order.SellerId);
            var order = model.Order;
            order.Id = Guid.NewGuid().ToString();
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

            var user = db.Users.FirstOrDefault(entry => entry.Id == order.UserId);
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
                    db.OrderProductOptions.Add(orderProductOption);
                }
            }

            //add transaction for personal purchase
            var transaction = new Transaction()
            {
                Id = Guid.NewGuid().ToString(),
                Bonuses = order.PersonalBonusesSum,
                BonusesBalans = user.CurrentBonusAccount + order.PersonalBonusesSum,
                OrderId = order.Id,
                PayeeId = user.Id,
                Time = DateTime.UtcNow
            };
            db.Transactions.Add(transaction);
            db.SaveChanges();
            Cart.Cart.CurrentInstance.Clear();
            var cartMumberCookie = HttpContext.Current.Response.Cookies["cartNumber"];
            if (cartMumberCookie != null)
            {
                cartMumberCookie.Expires = DateTime.UtcNow.AddDays(-1);
            }
        }
    }
}
