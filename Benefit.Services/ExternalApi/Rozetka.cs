using Benefit.Common.Extensions;
using Benefit.Common.Helpers;
using Benefit.DataTransfer.ApiDto.Rozetka;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.HttpClient;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Benefit.Services.ExternalApi
{
    public class RozetkaApiService : IMarketPlaceApi
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

        public void UpdateOrderStatus(string id, OrderStatus oldStatus, OrderStatus newStatus, string ttn, int tryCount = 1)
        {
            var success = true;
            var updateOrderUrl = SettingsService.Rozetka.BaseUrl + "orders/" + id;
            var authToken = GetAccessToken(SettingsService.Rozetka.UserName, SettingsService.Rozetka.Password);
            if (authToken == null)
            {
                success = false;
            }
            else
            {
                var updateOrderIngest = new UpdateOrderIngest
                {
                    status = (int)newStatus + 1
                };
                var postData = JsonConvert.SerializeObject(updateOrderIngest);
                var ordersResult = _httpClient.Post<BaseDto>(updateOrderUrl, postData, "application/json", authToken, "put");
                if (ordersResult.StatusCode == HttpStatusCode.OK)
                {
                    if (!ordersResult.Data.success)
                    {
                        success = false;
                    }
                    else
                    {
                        success = false;
                    }
                }
            }
            if (!success)
            {
                if (tryCount <= 3)
                {
                    Thread.Sleep(3000);
                    UpdateOrderStatus(id, oldStatus, newStatus, ttn, tryCount + 1);
                }
                else
                {
                    var notificationService = new NotificationsService();
                    notificationService.NotifyApiFailRequest(id, "Rozetka", oldStatus, newStatus);
                }
            }
        }

        public void RemoveOrderPurchases(Order order)
        {
            var updateOrderUrl = SettingsService.Rozetka.BaseUrl + "orders/" + order.ExternalId;
            var authToken = GetAccessToken(SettingsService.Rozetka.UserName, SettingsService.Rozetka.Password);
            if (authToken != null)
            {
                var updateOrderIngest = new UpdateOrderPurchasesIngest
                {
                    purchases = order.OrderProducts.Select(entry => new OrderProductQuantityIngest { id = entry.ExternalId, quantity = 0 }).ToList()
                };
                var postData = JsonConvert.SerializeObject(updateOrderIngest);
                var ordersResult = _httpClient.Post<BaseDto>(updateOrderUrl, postData, "application/json", authToken, "put");
                if (ordersResult.StatusCode == HttpStatusCode.OK)
                {
                    if (!ordersResult.Data.success)
                    {
                        _logger.Fatal("[Rozetka] update purchases order fail: " + order.ExternalId);
                    }
                }
            }
        }

        public void ProcessOrders(string getOrdersUrl = null, string authToken = null, int type = 1)
        {
            var orderSuffixRegex = new Regex(@"\(\b(\w*.?(bc|BC).+\d\w*)\b\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var orders = new List<Order>();
            if (getOrdersUrl == null)
            {
                getOrdersUrl = SettingsService.Rozetka.BaseUrl + "orders/search?expand=purchases,delivery&page=1&type=1";
                var lastOrder = db.Orders.Where(entry => entry.OrderType == OrderType.Rozetka).OrderByDescending(entry => entry.Time).FirstOrDefault();
                if (lastOrder != null)
                {
                    getOrdersUrl += string.Format("&created_from={0}", lastOrder.Time.ToString("yyyy-MM-dd"));
                }
                else
                {
                    getOrdersUrl += string.Format("&created_from={0}", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
                }
            }
            if (authToken == null)
            {
                authToken = GetAccessToken(SettingsService.Rozetka.UserName, SettingsService.Rozetka.Password);
            }
            if (authToken != null)
            {
                var ordersResult = _httpClient.Get<OrdersModelDto>(getOrdersUrl, authToken);
                if (ordersResult.StatusCode == HttpStatusCode.OK)
                {
                    var maxOrderNumber = db.Orders.Max(entry => entry.OrderNumber);
                    var rOrders = ordersResult.Data.content.orders.Where(entry => !db.Orders.Any(or => or.ExternalId == entry.id)).ToList();
                    foreach (var rOrder in rOrders)
                    {
                        var productNames = rOrder.purchases.Select(entry => orderSuffixRegex.Match(entry.item_name).Value.ToLower()).ToList();
                        var products = db.Products.Where(entry => productNames.Any(pn => entry.Name.ToLower().Contains(pn))).ToList();
                        var sellerIds = products.Select(entry => entry.SellerId).Distinct().ToList();
                        foreach (var sellerId in sellerIds)
                        {
                            var order = new Order()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ExternalId = rOrder.id,
                                OrderType = OrderType.Rozetka,
                                PaymentType = PaymentType.Cash,
                                Description = rOrder.comment,
                                SellerId = sellerId,
                                OrderNumber = ++maxOrderNumber,
                                Time = DateTime.Parse(rOrder.created),
                                Status = ((OrderStatus)rOrder.status - 1),
                                ShippingName = rOrder.delivery.delivery_service_name,
                                ShippingCost = rOrder.delivery.cost.GetValueOrDefault(0),
                                ShippingAddress = string.Format("{0}, {1} {2}", rOrder.delivery.city.title, rOrder.delivery.place_street, rOrder.delivery.place_house + " " + rOrder.delivery.place_flat),
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
                                        ExternalId = rProduct.id,
                                        OrderId = order.Id,
                                        ProductName = rProduct.item_name,
                                        ProductId = product.Id,
                                        ProductSku = product.SKU,
                                        ProductPrice = rProduct.price,
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
                    if (ordersResult.Data.content._meta.currentPage != ordersResult.Data.content._meta.pageCount && ordersResult.Data.content._meta.pageCount != 0)
                    {
                        getOrdersUrl = getOrdersUrl.Replace("page=" + ordersResult.Data.content._meta.currentPage, "page=" + (ordersResult.Data.content._meta.currentPage + 1));
                        ProcessOrders(getOrdersUrl, authToken);
                    }
                    else if (type < 3)
                    {
                        getOrdersUrl = getOrdersUrl.Replace("type=" + type, "type=" + (type + 1));
                        ProcessOrders(getOrdersUrl, authToken, ++type);
                    }
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

        public void ProcessOrders()
        {
            throw new NotImplementedException();
        }
    }
}
