using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.Services.Cart
{
    public class Cart
    {
        private string SessionKey;
        private static readonly DateTime AbsoluteExpiration = DateTime.Now.AddHours(6);

        public Order Order { get; set; }
        public static Cart CurrentInstance
        {
            get
            {
                var sessionKey = string.Format("{0}-{1}", DomainConstants.OrderPrefixKey, HttpContext.Current.Session.SessionID);
                var cart = HttpRuntime.Cache[sessionKey] as Cart ?? new Cart()
                {
                    SessionKey = sessionKey,
                    Order = new Order()
                };
                HttpRuntime.Cache.Add(sessionKey, cart, null, AbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                return cart;
            }
        }

        public int AddProduct(OrderProduct orderProduct, string sellerId)
        {
            if (Order.SellerId != null && Order.SellerId != sellerId)
            {
                Order.OrderProducts.Clear();
                Order.OrderProductOptions.Clear();
            }
            var existingProduct = Order.OrderProducts.FirstOrDefault(entry => entry.ProductId == orderProduct.ProductId);
            if (existingProduct != null)
            {
                existingProduct.Amount++;
            }
            else
            {
                using (var db = new ApplicationDbContext())
                {
                    var product =
                        db.Products.Include(entry => entry.Currency)
                            .FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                    orderProduct.ProductName = product.Name;
                    orderProduct.ProductPrice = (double)(product.Price * product.Currency.Rate);
                    foreach (var orderProductOption in orderProduct.OrderProductOptions)
                    {
                        var productOption = db.ProductOptions.Find(orderProductOption.ProductOptionId);
                        orderProductOption.ProductOptionName = productOption.Name;
                        orderProductOption.ProductOptionPriceGrowth = productOption.PriceGrowth;
                    }
                }
                Order.SellerId = sellerId;
                Order.OrderProducts.Add(orderProduct);
            }
            HttpRuntime.Cache.Add(SessionKey, this, null, AbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            return (int)Order.OrderProducts.Sum(entry=>entry.Amount);
        }

        public int RemoveProduct(string id)
        {
            var productToRemove = Order.OrderProducts.FirstOrDefault(entry => entry.ProductId == id);
            Order.OrderProducts.Remove(productToRemove);
            HttpRuntime.Cache.Add(SessionKey, this, null, AbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            return Order.OrderProducts.Count;
        }

        public void Clear()
        {
            Order = new Order();
        }

        public double GetOrderSum()
        {
            var sum = Order.OrderProducts.Sum(
                    entry =>
                        entry.ProductPrice * entry.Amount +
                        entry.OrderProductOptions.Sum(option => option.ProductOptionPriceGrowth * option.Amount));
            return sum;
        }
    }
}
