using Benefit.Common.Extensions;
using Benefit.DataTransfer.ApiDto.Allo;
using Benefit.Domain;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.HttpClient;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Benefit.Services.ExternalApi
{
    public class AlloApiService : BaseMarketPlaceApi
    {
        private BenefitHttpClient _httpClient = new BenefitHttpClient();
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private NotificationsService _notificationService = new NotificationsService();

        public override string GetAccessToken(string userName, string password)
        {
            var auth = new AuthIngest
            {
                username = userName,
                apiKey = password
            };
            var authUrl = SettingsService.Allo.BaseUrl + "login";
            var postData = JsonConvert.SerializeObject(auth);
            var authResult = _httpClient.Post<AuthDto>(authUrl, postData, "application/json", null, "post", new Dictionary<string, string> { { "api_version", "1" } });
            if (authResult.StatusCode == HttpStatusCode.OK)
            {
                return authResult.Data.sessionId;
            }
            return null;
        }
        public override void UpdateOrderStatus(string id, OrderStatus oldStatus, OrderStatus newStatus, string ttn, int tryCount = 1, string sellerComment = null)
        {
            var success = true;
            var updateOrderUrl = SettingsService.Allo.BaseUrl + "call?apiPath=orders.update";
            var authToken = GetAccessToken(SettingsService.Allo.UserName, SettingsService.Allo.ApiKey);
            var status = SettingsService.Allo.ReverseOrderStatusMapping[newStatus];
            var updateOrderIngest = new UpdateOrderIngest
            {
                sessionId = authToken,
                args = new UpdateOrderIngestArgs
                {
                    orders = new List<UpdateOrderIngestOrder>
                    {
                        new UpdateOrderIngestOrder
                        {
                            id = id,
                            tracking_number = ttn,
                            updated_date = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                            status = new UpdateOrderIngestStatus
                            {
                                main_status = status
                            }
                        }
                    }
                }
            };
            var postData = JsonConvert.SerializeObject(updateOrderIngest);
            var ordersResult = _httpClient.Post<UpdateOrdersDto>(updateOrderUrl, postData, "application/json", authToken, "put");
            if (ordersResult.StatusCode == HttpStatusCode.OK)
            {
                if (ordersResult.Data.orders[0].status != "success")
                {
                    success = false;
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
                    notificationService.NotifyApiFailRequest(id, "Allo", oldStatus, newStatus);
                }
            }
        }

        //public void RemoveOrderPurchases(Order order)
        //{
        //    var updateOrderUrl = SettingsService.Rozetka.BaseUrl + "orders/" + order.ExternalId;
        //    var authToken = GetAccessToken(SettingsService.Rozetka.UserName, SettingsService.Rozetka.Password);
        //    if (authToken != null)
        //    {
        //        var updateOrderIngest = new UpdateOrderPurchasesIngest
        //        {
        //            purchases = order.OrderProducts.Select(entry => new OrderProductQuantityIngest { id = entry.ExternalId, quantity = 0 }).ToList()
        //        };
        //        var postData = JsonConvert.SerializeObject(updateOrderIngest);
        //        var ordersResult = _httpClient.Post<BaseDto>(updateOrderUrl, postData, "application/json", authToken, "put");
        //        if (ordersResult.StatusCode == HttpStatusCode.OK)
        //        {
        //            if (!ordersResult.Data.success)
        //            {
        //                _logger.Fatal("[Rozetka] update purchases order fail: " + order.ExternalId);
        //            }
        //        }
        //    }
        //}

        public override async Task ProcessOrders(string getOrdersUrl = null, string authToken = null, int type = 1, int offset = 0)
        {
            using (var db = new ApplicationDbContext())
            {
                var ingest = new OrdersIngest()
                {
                    args = new OrdersIngestArgs { limit = limit, offset = offset }
                };
                var orderSuffixRegex = new Regex(SettingsService.MarketplaceApi.OrderSuffixRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var orders = new List<Order>();
                getOrdersUrl = SettingsService.Allo.BaseUrl + "call?apiPath=orders.orderList";
                var lastOrder = db.Orders.Where(entry => entry.OrderType == OrderType.Allo).OrderByDescending(entry => entry.Time).FirstOrDefault();
                if (lastOrder != null)
                {
                    ingest.args.accepted_from = lastOrder.Time.ToString("yyyy-MM-dd hh:mm:ss");
                }
                else
                {
                    ingest.args.accepted_from = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd hh:mm:ss");
                }
                if (authToken == null)
                {
                    authToken = GetAccessToken(SettingsService.Allo.UserName, SettingsService.Allo.ApiKey);
                }
                if (authToken != null)
                {
                    ingest.sessionId = authToken;
                    var postData = JsonConvert.SerializeObject(ingest);
                    var ordersResult = _httpClient.Post<OrdersDto>(getOrdersUrl, postData, "application/json");
                    if (ordersResult.StatusCode == HttpStatusCode.OK)
                    {
                        var maxOrderNumber = db.Orders.Max(entry => entry.OrderNumber);
                        var rOrders = ordersResult.Data.orders.Where(entry => !db.Orders.Any(or => or.ExternalId == entry.id && or.OrderType == OrderType.Allo)).ToList();
                        foreach (var rOrder in rOrders)
                        {
                            var products = new List<Product>();
                            foreach (var entry in rOrder.products)
                            {
                                var productNameSuffix = orderSuffixRegex.Match(entry.name).Value.ToLower();
                                productNameSuffix = productNameSuffix == string.Empty ? null : productNameSuffix;
                                var product = db.Products.FirstOrDefault(x => x.Name.ToLower().Contains(productNameSuffix));
                                if (product == null)
                                {
                                    await _notificationService.NotifyApiFailRequest(string.Format("Не знайдено товару в локальній базі даних. № Замовлення на Allo: {0}, назва товару: {1}", rOrder.id, entry.name)).ConfigureAwait(false);
                                }
                                else
                                {
                                    products.Add(product);
                                }
                            }
                            var sellerIds = products.Select(entry => entry.SellerId).Distinct().ToList();
                            foreach (var sellerId in sellerIds)
                            {
                                var order = new Order()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    ExternalId = rOrder.id,
                                    OrderType = OrderType.Allo,
                                    Description = rOrder.payment_type,
                                    SellerId = sellerId,
                                    OrderNumber = ++maxOrderNumber,
                                    Time = DateTime.Parse(rOrder.created_date),
                                    Status = SettingsService.Allo.OrderStatusMapping[rOrder.status.status],
                                    ShippingName = rOrder.shipping.type,
                                    ShippingTrackingNumber = rOrder.shipping.tracking_number,
                                    PersonalBonusesSum = 0,
                                    PointsSum = 0,
                                    LastModified = DateTime.UtcNow,
                                    UserName = string.Format("{0} {1}", rOrder.customer.firstname, rOrder.customer.lastname),
                                    UserPhone = rOrder.customer.telephone
                                };
                                if (rOrder.note != null)
                                {
                                    var stamp = new OrderStatusStamp()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        OrderId = order.Id,
                                        OrderStatus = order.Status,
                                        Time = DateTime.UtcNow,
                                        Comment = rOrder.note.Truncate(256),
                                        UpdatedBy = "Allo"
                                    };
                                    db.OrderStatusStamps.Add(stamp);
                                }
                                double shippingPrice = 0;
                                double.TryParse(rOrder.shipping.price, out shippingPrice);
                                order.ShippingCost = shippingPrice;
                                if (rOrder.shipping.stock != null)
                                {
                                    order.ShippingAddress = string.Format("{0} ({1}) {2}", rOrder.shipping.city, rOrder.shipping.region_name, rOrder.shipping.stock.name);
                                }
                                if (rOrder.shipping.address != null)
                                {
                                    order.ShippingAddress = string.Format("{0}, {1}, {2} {3}", rOrder.shipping.address.city, rOrder.shipping.address.street, rOrder.shipping.address.house, rOrder.shipping.address.apartment);
                                }
                                switch (rOrder.payment_type_id)
                                {
                                    case "checkmo":
                                        order.PaymentType = PaymentType.Cash;
                                        break;
                                    case "wayforpay_payment":
                                        order.PaymentType = PaymentType.Acquiring;
                                        break;
                                    case "masterpass":
                                        order.PaymentType = PaymentType.Acquiring;
                                        break;
                                    case "applepay":
                                        order.PaymentType = PaymentType.Acquiring;
                                        break;
                                    case "googlepay":
                                        order.PaymentType = PaymentType.Acquiring;
                                        break;
                                    default:
                                        order.PaymentType = PaymentType.Cash;
                                        break;
                                }
                                foreach (var rProduct in rOrder.products)
                                {
                                    var product = products.FirstOrDefault(entry => entry.Name == rProduct.name);
                                    var image = product.Images.OrderBy(entry => entry.Order).FirstOrDefault();
                                    var orderProduct = new OrderProduct()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        ExternalId = rProduct.sku,
                                        OrderId = order.Id,
                                        ProductName = rProduct.name,
                                        ProductId = product.Id,
                                        ProductSku = product.SKU,
                                        ProductPrice = rProduct.price,
                                        Amount = rProduct.quantity,
                                        ProductImageUrl = image == null ? null : image.ImageUrl
                                    };
                                    order.OrderProducts.Add(orderProduct);
                                }
                                order.Sum = order.GetOrderSum();
                                orders.Add(order);
                            }
                        }
                        db.Orders.AddRange(orders);
                        db.SaveChanges();
                        if (ordersResult.Data.total_records == limit)
                        {
                            await ProcessOrders(getOrdersUrl, authToken, 1, offset + limit);
                        }
                    }
                    else
                    {
                        _logger.Error("Allo get orders request fail");
                    }
                }
                else
                {
                    _logger.Error("Allo auth token is null");
                }
            }
            HttpRuntime.Cache["IsAlloProcessing"] = false;
        }
    }
}
