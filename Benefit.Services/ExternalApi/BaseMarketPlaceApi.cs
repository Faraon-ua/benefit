using Benefit.Common.Extensions;
using Benefit.Domain;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Benefit.Services.ExternalApi
{
    public enum MarketplaceType
    {
        Rozetka,
        Allo
    }
    public abstract class BaseMarketPlaceApi
    {
        public const int limit = 50;
        protected NotificationsService _notificationService = new NotificationsService();
        public abstract string GetAccessToken(string userName, string password);
        public abstract void UpdateOrderStatus(string id, OrderStatus oldStatus, OrderStatus newStatus, string ttn, int tryCount = 1, string sellerComment = null);
        public abstract Task ProcessOrders(string getOrdersUrl = null, string authToken = null, int type = 1, int offset = 0);
        public static BaseMarketPlaceApi GetMarketplaceServiceInstance(MarketplaceType type)
        {
            switch (type)
            {
                case MarketplaceType.Rozetka:
                    return new RozetkaApiService();
                case MarketplaceType.Allo:
                    return new AlloApiService();
            }
            return null;
        }
        public static BaseMarketPlaceApi GetMarketplaceServiceInstance(OrderType type)
        {
            switch (type)
            {
                case OrderType.Rozetka:
                    return new RozetkaApiService();
                case OrderType.Allo:
                    return new AlloApiService();
            }
            return null;
        }
        protected void ProcessOrder(OrderProduct baseOrderProduct, Order baseOrder, List<Order> ordersBySeller, ref int maxOrderNumber, ApplicationDbContext db)
        {
            var orderSuffixRegex = new Regex(SettingsService.MarketplaceApi.OrderSuffixRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var suffix = orderSuffixRegex.Match(baseOrderProduct.ProductName).Value.ToLower();
            var product = string.IsNullOrEmpty(suffix) ? null : db.Products.FirstOrDefault(entry => entry.Name.ToLower().Contains(suffix));
            if (product == null)
            {
                Task.Run(() => _notificationService.NotifyApiFailRequest(string.Format("Не знайдено товару в локальній базі даних. № Замовлення на {0}: {1}, назва товару: {2}", baseOrder.OrderType.ToString(), baseOrder.ExternalId, baseOrderProduct.ProductName)));
            }
            else
            {
                var image = product.Images.OrderBy(entry => entry.Order).FirstOrDefault();
                var order = ordersBySeller.FirstOrDefault(entry => entry.SellerId == product.SellerId);
                if (order == null)
                {
                    order = new Order()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ExternalId = baseOrder.Id,
                        OrderType = baseOrder.OrderType,
                        PaymentType = baseOrder.PaymentType,
                        Description = baseOrder.Description,
                        SellerId = product.SellerId,
                        OrderNumber = ++maxOrderNumber,
                        Time = baseOrder.Time,
                        ShippingName = baseOrder.ShippingName,
                        ShippingAddress = baseOrder.ShippingAddress,
                        ShippingCost = baseOrder.ShippingCost,
                        ShippingTrackingNumber = baseOrder.ShippingTrackingNumber,
                        PersonalBonusesSum = 0,
                        PointsSum = 0,
                        LastModified = DateTime.UtcNow,
                        UserName = baseOrder.UserName,
                        UserPhone = baseOrder.UserPhone,
                        Status = baseOrder.Status
                    };
                    if (baseOrder.Description != null)
                    {
                        var stamp = new StatusStamp()
                        {
                            Id = Guid.NewGuid().ToString(),
                            OrderId = order.Id,
                            Status = (int)order.Status,
                            Time = DateTime.UtcNow,
                            Comment = baseOrder.Description,
                            UpdatedBy = baseOrder.OrderType.ToString()
                        };
                        db.StatusStamps.Add(stamp);
                    }

                    ordersBySeller.Add(order);
                }
                var orderProduct = new OrderProduct()
                {
                    Id = Guid.NewGuid().ToString(),
                    ExternalId = baseOrderProduct.ExternalId,
                    OrderId = order.Id,
                    ProductName = baseOrderProduct.ProductName,
                    ProductId = product.Id,
                    ProductSku = product.SKU,
                    ProductPrice = baseOrderProduct.ProductPrice,
                    Amount = baseOrderProduct.Amount,
                    ProductImageUrl = image == null ? null : image.ImageUrl
                };
                order.OrderProducts.Add(orderProduct);
                order.Sum = order.GetOrderSum();
            }
        }
    }
}
