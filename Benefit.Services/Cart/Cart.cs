﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using NLog;

namespace Benefit.Services.Cart
{
    public class Cart
    {
        private string SessionKey;
        private static readonly DateTime AbsoluteExpiration = DateTime.Now.AddHours(6);
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public Order Order { get; set; }
        public static Cart CurrentInstance
        {
            get
            {
                var sessionKey = string.Format("{0}-{1}", DomainConstants.OrderPrefixKey, HttpContext.Current.Session.SessionID);
                _logger.Info("Cart.CurrentInstance.SessionKey:{0}", sessionKey);
                var cart = HttpRuntime.Cache[sessionKey] as Cart;
                if (HttpRuntime.Cache[sessionKey] == null)
                {
                    cart = new Cart()
                    {
                        SessionKey = sessionKey,
                        Order = new Order()
                    };
                    HttpRuntime.Cache.Add(sessionKey, cart, null, AbsoluteExpiration, Cache.NoSlidingExpiration,
                        CacheItemPriority.Normal, null);
                }
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
            _logger.Info("Cart.AddProduct.SessionKey:{0}, Cart.AddProduct.OrderProduct.Name:{1}", SessionKey, orderProduct.ProductName);
            HttpRuntime.Cache.Add(SessionKey, this, null, AbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            return GetOrderProductsCount();
        }

        public int RemoveProduct(string id)
        {
            var productToRemove = Order.OrderProducts.FirstOrDefault(entry => entry.ProductId == id);
            Order.OrderProducts.Remove(productToRemove);
            HttpRuntime.Cache.Add(SessionKey, this, null, AbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            return GetOrderProductsCount();
        }

        public void Clear()
        {
            Order = new Order();
        }

        public double GetOrderSum()
        {
            return Order.GetOrderSum();
        }

        private int GetOrderProductsCount()
        {
            var number = 0;
            foreach (var product in Order.OrderProducts)
            {
                if (product.IsWeightProduct) number++;
                else
                {
                    number += (int)product.Amount;
                }
            }
            return number;
        }
    }
}
