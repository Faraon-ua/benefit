using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using NLog;

namespace Benefit.Services.Cart
{
    public class Cart
    {
        private static readonly TimeSpan SlidingExpiration = new TimeSpan(6, 0, 0);
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public string SessionKey;

        public Order Order { get; set; }
        public static Cart CurrentInstance
        {
            get
            {
                var sessionKey = string.Format("{0}-{1}", DomainConstants.OrderPrefixKey, HttpContext.Current.Session.SessionID);
                var cart = HttpRuntime.Cache[sessionKey] as Cart;
                if (cart == null)
                {
                    cart = new Cart()
                    {
                        SessionKey = sessionKey,
                        Order = new Order()
                    };
                    HttpRuntime.Cache.Insert(sessionKey, cart, null, Cache.NoAbsoluteExpiration, SlidingExpiration);
                }
                return cart;
            }
        }

        public CartEditResult AddProduct(OrderProduct orderProduct, string sellerId)
        {
            if (Order.SellerId != null && Order.SellerId != sellerId)
            {
                Order.OrderProducts.Clear();
                Order.OrderProductOptions.Clear();
            }
            var existingProduct = Order.OrderProducts.FirstOrDefault(entry => entry.ProductId == orderProduct.ProductId);
            if (existingProduct != null)
            {
                existingProduct.Amount += orderProduct.Amount;
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
            HttpRuntime.Cache.Insert(SessionKey, this, null, Cache.NoAbsoluteExpiration, SlidingExpiration);
            return GetOrderProductsCountAndPrice();
        }

        public CartEditResult RemoveProduct(string id)
        {
            var productToRemove = Order.OrderProducts.FirstOrDefault(entry => entry.ProductId == id);
            Order.OrderProducts.Remove(productToRemove);
            HttpRuntime.Cache.Insert(SessionKey, this, null, Cache.NoAbsoluteExpiration, SlidingExpiration);            
            return GetOrderProductsCountAndPrice();
        }

        public void Clear()
        {
            Order = new Order();
        }

        public double GetOrderSum()
        {
            return Order.GetOrderSum();
        }

        public CartEditResult GetOrderProductsCountAndPrice()
        {
            var result = new CartEditResult()
            {
                ProductsNumber = 0,
                Price = 0
            };
            foreach (var product in Order.OrderProducts)
            {
                if (product.IsWeightProduct) result.ProductsNumber++;
                else
                {
                    result.ProductsNumber += (int)product.Amount;
                }
                result.Price += product.ProductPrice*product.Amount + (product.OrderProductOptions.Sum(entry=>entry.ProductOptionPriceGrowth * entry.Amount));
            }
            return result;
        }
    }
}
