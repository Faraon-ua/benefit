using Benefit.Common.Extensions;
using Benefit.Common.Helpers;
using Benefit.DataTransfer.ApiDto.Rozetka;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.HttpClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Benefit.Services.ExternalApi
{
    public class RozetkaApiService
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private BenefitHttpClient _httpClient = new BenefitHttpClient();
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public string GetAccessToken(string userName, string password)
        {
            var auth = new AuthIngest
            {
                username = userName,
                password = Encoding.UTF8.EncodeBase64(password)
            };
            var authForm = ContentTypeConvert.SerializeToXwwwFormUrlencoded(auth);
            var authUrl = SettingsService.Rozetka.BaseUrl + "sites";
            var authResult = _httpClient.Post<AuthDto>(authUrl, authForm, "application/x-www-form-urlencoded");
            if (authResult.StatusCode == HttpStatusCode.OK)
            {
                return authResult.Data.content.access_token;
            }
            return null;
        }

        public void ProcessOrders()
        {
            var orders = new List<Order>();
            var getOrdersUrl = SettingsService.Rozetka.BaseUrl + "orders/search?expand=purchases,delivery";
            var lastOrder = db.Orders.Where(entry => entry.OrderType == OrderType.Rozetka).OrderByDescending(entry => entry.Time).FirstOrDefault();
            if (lastOrder != null)
            {
                getOrdersUrl += string.Format("&created_from={0}", lastOrder.Time.ToString("yyyy-MM-dd"));
            }
            else
            {
                getOrdersUrl += string.Format("&created_from={0}", DateTime.Now.AddDays(-1).ToString("YYYY-MM-DD"));
            }
            var authToken = GetAccessToken(SettingsService.Rozetka.UserName, SettingsService.Rozetka.Password);
            if (authToken != null)
            {
                var ordersResult = _httpClient.Get<OrdersModelDto>(getOrdersUrl, authToken);
                if (ordersResult.StatusCode == HttpStatusCode.OK)
                {
                    var maxOrderNumber = db.Orders.Max(entry => entry.OrderNumber);
                    foreach (var rOrder in ordersResult.Data.content.orders)
                    {
                        var productNames = rOrder.purchases.Select(entry => entry.item_name).ToList();
                        var products = db.Products.Where(entry => productNames.Contains(entry.Name)).ToList();
                        var sellerIds = products.Select(entry => entry.SellerId).Distinct().ToList();
                        foreach (var sellerId in sellerIds)
                        {
                            var order = new Order()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ExternalId = rOrder.id,
                                OrderType = OrderType.Rozetka,
                                PaymentType = PaymentType.PrePaid,
                                Description = rOrder.comment,
                                SellerId = sellerId,
                                OrderNumber = ++maxOrderNumber,
                                Time = DateTime.UtcNow,
                                Status = OrderStatus.Created,
                                ShippingName = rOrder.delivery.delivery_service_name,
                                ShippingCost = rOrder.delivery.cost.GetValueOrDefault(0),
                                ShippingAddress = string.Format("{0}, {1} {2}", rOrder.delivery.city.title, rOrder.delivery.place_street, rOrder.delivery.place_house + " "+ rOrder.delivery.place_flat),
                                PersonalBonusesSum = 0,
                                PointsSum = 0,
                                LastModified = DateTime.UtcNow,
                                UserName = rOrder.delivery.recipient_title,
                                UserPhone = rOrder.user_phone
                            };
                            foreach (var rProduct in rOrder.purchases)
                            {
                                var product = products.FirstOrDefault(entry => entry.Name == rProduct.item_name);
                                if (product != null)
                                {
                                    var image = product.Images.OrderBy(entry => entry.Order).FirstOrDefault();
                                    var orderProduct = new OrderProduct()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        OrderId = order.Id,
                                        ProductName = rProduct.item_name,
                                        ProductId = product.Id,
                                        ProductSku = product.SKU,
                                        ProductPrice = rProduct.cost,
                                        Amount = rProduct.quantity,
                                        ProductImageUrl = image == null ? null : image.ImageUrl
                                    };
                                    order.OrderProducts.Add(orderProduct);
                                }
                            }
                            order.Sum = order.GetOrderSum();
                            orders.Add(order);
                        }
                    }
                    db.Orders.AddRange(orders);
                    db.SaveChanges();
                }
                else
                {
                    _logger.Error("Rozetka get orders request fail");
                }
            }
            else
            {
                _logger.Error("Rozetka auth token is null");
            }
        }
    }
}
