using System;
using System.Collections.Generic;
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

        public List<Order> Orders { get; set; }
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
                        Orders = new List<Order>()
                    };
                    HttpRuntime.Cache.Insert(sessionKey, cart, null, Cache.NoAbsoluteExpiration, SlidingExpiration);
                }
                return cart;
            }
        }

        public CartEditResult AddProduct(OrderProduct orderProduct, string sellerId)
        {
            var order = Orders.FirstOrDefault(entry => entry.SellerId == sellerId);
            if (order == null)
            {
                order = new Order()
                {
                    SellerId = sellerId
                };
                Orders.Add(order);
            }
            var existingProduct = order.OrderProducts.FirstOrDefault(entry => entry.ProductId == orderProduct.ProductId);
            if (existingProduct != null)
            {
                existingProduct.Amount += orderProduct.Amount;
            }
            else
            {
                using (var db = new ApplicationDbContext())
                {
                    var product =
                        db.Products
                            .Include(entry => entry.Seller)
                            .Include(entry => entry.Currency)
                            .Include(entry => entry.Images)
                            .FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                    orderProduct.ProductName = product.Name;
                    orderProduct.ProductUrlName = product.UrlName;
                    orderProduct.ProductSku = product.SKU;
                    var image = product.Images.OrderBy(entry => entry.Order).FirstOrDefault();
                    orderProduct.ProductImageUrl = image == null ? null : image.ImageUrl;
                    orderProduct.ProductPrice = product.Price * product.Currency.Rate;
                    if (product.WholesalePrice.HasValue && product.WholesaleFrom.HasValue)
                    {
                        orderProduct.WholesaleProductPrice = product.WholesalePrice.Value * product.Currency.Rate;
                        orderProduct.WholesaleFrom = product.WholesaleFrom.Value;
                    }
                    foreach (var orderProductOption in orderProduct.OrderProductOptions)
                    {
                        var productOption = db.ProductOptions.Find(orderProductOption.ProductOptionId);
                        orderProductOption.ProductOptionName = productOption.Name;
                        orderProductOption.ProductOptionPriceGrowth = productOption.PriceGrowth;
                        orderProductOption.EditableAmount = productOption.EditableAmount;
                    }
                    order.SellerUrlName = product.Seller.UrlName;
                    order.SellerName = product.Seller.Name;
                    order.SellerPrimaryRegionName = product.Seller.PrimaryRegionName;
                    order.SellerUserDiscount = product.Seller.UserDiscount;
                }
                order.SellerId = sellerId;
                order.OrderProducts.Add(orderProduct);
            }
            HttpRuntime.Cache.Insert(SessionKey, this, null, Cache.NoAbsoluteExpiration, SlidingExpiration);
            return GetOrderProductsCountAndPrice();
        }

        public CartEditResult UpdatePoductQuantity(string productId, string sellerId, double amount)
        {
            var order = Orders.FirstOrDefault(entry => entry.SellerId == sellerId);
            var product = order.OrderProducts.FirstOrDefault(entry => entry.ProductId == productId);
            product.Amount = amount;
            return GetOrderProductsCountAndPrice();
        }

        public CartEditResult RemoveProduct(string sellerId, string productId)
        {
            var order = Orders.FirstOrDefault(entry => entry.SellerId == sellerId);
            var productToRemove = order.OrderProducts.FirstOrDefault(entry => entry.ProductId == productId);
            order.OrderProducts.Remove(productToRemove);
            HttpRuntime.Cache.Insert(SessionKey, this, null, Cache.NoAbsoluteExpiration, SlidingExpiration);
            return GetOrderProductsCountAndPrice();
        }

        public void ClearSellerOrder(string sellerId)
        {
            Orders.Remove(Orders.FirstOrDefault(entry => entry.SellerId == sellerId));
        }

        public double GetOrderSum()
        {
            return Orders.Sum(entry => entry.GetOrderSum());
        }

        public CartEditResult GetOrderProductsCountAndPrice()
        {
            var result = new CartEditResult()
            {
                ProductsNumber = 0,
                Price = 0
            };
            foreach (var order in Orders)
            {
                foreach (var product in order.OrderProducts)
                {
                    if (product.IsWeightProduct) result.ProductsNumber++;
                    else
                    {
                        result.ProductsNumber += (int)product.Amount;
                    }
                    var productTotal = (product.WholesaleProductPrice.HasValue && product.WholesaleFrom.HasValue &&
                                        product.Amount >= product.WholesaleFrom.Value)
                        ? product.WholesaleProductPrice.Value
                        : product.ProductPrice;

                    result.Price += productTotal * product.Amount + (product.OrderProductOptions.Sum(entry => entry.ProductOptionPriceGrowth * entry.Amount));
                }
            }

            return result;
        }
    }
}
