using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace Benefit.Services.Cart
{
    public class Cart
    {
        private static readonly TimeSpan SlidingExpiration = new TimeSpan(6, 0, 0);
        private ApplicationDbContext db = new ApplicationDbContext();
        public static string SessionKey
        {
            get
            {
                return string.Format("{0}-{1}", DomainConstants.OrderPrefixKey, HttpContext.Current.Session.SessionID);
            }
        }
        public List<OrderVM> Orders { get; set; }
        private static Cart _cart;
        public static Cart CurrentInstance
        {
            get
            {
                if (_cart == null)
                {
                    _cart = new Cart();
                }
                _cart.Orders = HttpRuntime.Cache[SessionKey] as List<OrderVM>;
                if (_cart.Orders == null)
                {
                    _cart.Orders = new List<OrderVM>();
                    HttpRuntime.Cache.Insert(SessionKey, _cart.Orders, null, Cache.NoAbsoluteExpiration, SlidingExpiration);
                }
                return _cart;
            }
        }

        public CartEditResult AddProduct(OrderProduct orderProduct, string sellerId)
        {
            var seller = db.Sellers.Find(sellerId);
            var order = Orders.FirstOrDefault(entry => entry.SellerId == sellerId || entry.SellerId == seller.AssociatedSellerId);
            if (order == null)
            {
                order = new OrderVM()
                {
                    SellerId = sellerId
                };
                Orders.Add(order);
            }

            var productOptionIds = orderProduct.OrderProductOptions.Select(entry => entry.ProductOptionId).ToList();
            var existingProduct = order.OrderProducts.FirstOrDefault(entry =>
                entry.ProductId == orderProduct.ProductId && 
                entry.NameSuffix == orderProduct.NameSuffix &&
                entry.OrderProductOptions.Count == productOptionIds.Count &&
                entry.OrderProductOptions.All(po=>productOptionIds.Contains(po.ProductOptionId)));
            if (existingProduct != null)
            {
                existingProduct.Amount += orderProduct.Amount;
            }
            else
            {
                var product =
                    db.Products
                        .Include(entry => entry.Seller.SellerCategories)
                        .Include(entry => entry.Seller.ShippingMethods.Select(sh=>sh.Region))
                        .Include(entry => entry.Currency)
                        .Include(entry => entry.Images)
                        .Include(entry => entry.Category)
                        .FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                var regionId = RegionService.GetRegionId();
                var isAvailable =
                    product.Seller.ShippingMethods.Any(
                        entry => entry.RegionId == RegionConstants.AllUkraineRegionId || entry.RegionId == regionId);
                if (!isAvailable)
                {
                    var result = new CartEditResult() {
                        Comment = string.Join(",", product.Seller.ShippingMethods.Select(entry => entry.Region).Distinct(new RegionComparer()).Select(entry => entry.Name_ua))
                    };
                    return result;
                }
                orderProduct.ProductName = product.Name + orderProduct.NameSuffix;
                orderProduct.ProductSku = product.SKU;
                if (!string.IsNullOrEmpty(product.ExternalId))
                {
                    orderProduct.ProductName += string.Format(" ({0})", product.ExternalId);
                }
                orderProduct.SellerId = product.SellerId;
                orderProduct.CategoryName = product.Category.Name;
                var sellerCategory =
                    product.Seller.SellerCategories.FirstOrDefault(entry => entry.CategoryId == product.CategoryId) ??
                    product.Seller.SellerCategories.FirstOrDefault(entry =>
                        entry.CategoryId == product.Category.MappedParentCategoryId);

                orderProduct.ProductUrlName = product.UrlName;
                orderProduct.ProductSku = product.SKU;
                var image = product.Images.OrderBy(entry => entry.Order).FirstOrDefault();
                orderProduct.ProductImageUrl = image == null ? null : image.ImageUrl;
                if (product.Currency != null)
                {
                    orderProduct.ProductPrice = product.Price * product.Currency.Rate;
                }
                else
                {
                    orderProduct.ProductPrice = product.Price;
                }

                orderProduct.ProductPrice += orderProduct.PriceGrowth;
                if (product.WholesalePrice.HasValue && product.WholesaleFrom.HasValue)
                {
                    if (product.Currency != null)
                    {
                        orderProduct.WholesaleProductPrice = product.WholesalePrice.Value * product.Currency.Rate;
                    }
                    else
                    {
                        orderProduct.WholesaleProductPrice = product.WholesalePrice.Value;
                    }
                    orderProduct.WholesaleFrom = product.WholesaleFrom.Value;
                }
                //fetch amount of bonuses to be acquired for this product
                int totalBonusesDiscount = seller.TotalDiscount;
                if (sellerCategory != null)
                {
                    if (sellerCategory.CustomDiscount.HasValue)
                    {
                        totalBonusesDiscount = (int)sellerCategory.CustomDiscount;
                    }
                    if (sellerCategory.CustomMargin.HasValue)
                    {
                        orderProduct.ProductPrice += orderProduct.ProductPrice * sellerCategory.CustomMargin.Value / 100;
                    }
                }
                var userDiscount = totalBonusesDiscount <= 10
                    ? totalBonusesDiscount / 2
                    : 5 + totalBonusesDiscount - 10;
                orderProduct.BonusesAcquired = orderProduct.ActualPrice * userDiscount / 100;
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
                order.SellerId = seller.AssociatedSellerId == null ? sellerId : seller.AssociatedSellerId;
                order.OrderProducts.Add(orderProduct);
            }
            HttpRuntime.Cache.Insert(SessionKey, this.Orders, null, Cache.NoAbsoluteExpiration, SlidingExpiration);
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
            HttpRuntime.Cache.Insert(SessionKey, this.Orders, null, Cache.NoAbsoluteExpiration, SlidingExpiration);
            return GetOrderProductsCountAndPrice();
        }

        public void ClearSellerOrder(string sellerId)
        {
            Orders.Remove(Orders.FirstOrDefault(entry => entry.SellerId == sellerId));
            HttpRuntime.Cache.Insert(SessionKey, this.Orders, null, Cache.NoAbsoluteExpiration, SlidingExpiration);
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
                    if (product.IsWeightProduct)
                    {
                        result.ProductsNumber++;
                    }
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
