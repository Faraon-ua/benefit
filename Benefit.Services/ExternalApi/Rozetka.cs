using Benefit.Common.Constants;
using Benefit.Common.Extensions;
using Benefit.Common.Helpers;
using Benefit.DataTransfer.ApiDto.Rozetka;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Benefit.Services.ExternalApi
{
    public class RozetkaApiService : BaseMarketPlaceApi
    {
        private BenefitHttpClient _httpClient = new BenefitHttpClient();
        private Logger _logger = LogManager.GetCurrentClassLogger();

        private Order MapToDbOrder(OrderDto rOrder)
        {
            var order = AutoMapper.Mapper.Map<Order>(rOrder);
            order.OrderType = OrderType.Rozetka;
            switch (rOrder.payment_type)
            {
                case "cash":
                    order.PaymentType = PaymentType.Cash;
                    break;
                case "card":
                case "credit":
                case "part_pay":
                case "apple_pay":
                case "google_pay":
                case "instantly_pay":
                case "instantly_pay_promo":
                case "no_cash":
                case "privat24":
                    order.PaymentType = PaymentType.Acquiring;
                    break;
                default:
                    order.PaymentType = PaymentType.Cash;
                    break;
            }
            return order;
        }
        public override string GetAccessToken(string userName, string password)
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

        public override void UpdateOrderStatus(string id, OrderStatus oldStatus, OrderStatus newStatus, string ttn, int tryCount = 1, string sellerComment = null)
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
                    status = (int)newStatus + 1,
                    seller_comment = sellerComment
                };
                var postData = JsonConvert.SerializeObject(updateOrderIngest);
                var ordersResult = _httpClient.Post<BaseDto>(updateOrderUrl, postData, "application/json", authToken, "put");
                if (ordersResult.StatusCode == HttpStatusCode.OK)
                {
                    success = ordersResult.Data.success;
                }
            }
            if (!success)
            {
                if (tryCount <= 3)
                {
                    Thread.Sleep(3000);
                    UpdateOrderStatus(id, oldStatus, newStatus, ttn, tryCount + 1, sellerComment);
                }
                else
                {
                    _notificationService.NotifyApiFailRequest(id, "Rozetka", oldStatus, newStatus);
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

        public override async Task ProcessOrders(string getOrdersUrl = null, string authToken = null, int type = 1, int offset = 0)
        {
            using (var db = new ApplicationDbContext())
            {
                var orderSuffixRegex = new Regex(SettingsService.MarketplaceApi.OrderSuffixRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var orders = new List<Order>();
                if (getOrdersUrl == null)
                {
                    getOrdersUrl = SettingsService.Rozetka.BaseUrl + "orders/search?expand=purchases,delivery,payment_type&page=1&type=1";
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
                        var rOrders = ordersResult.Data.content.orders.Where(entry => !db.Orders.Any(or => or.ExternalId == entry.id && or.OrderType == OrderType.Rozetka)).ToList();
                        foreach (var rOrder in rOrders)
                        {
                            var baseOrder = MapToDbOrder(rOrder);
                            var ordersBySellers = new List<Order>();
                            foreach (var rProduct in rOrder.purchases)
                            {
                                var baseOrderProduct = AutoMapper.Mapper.Map<OrderProduct>(rProduct);
                                ProcessOrder(baseOrderProduct, baseOrder, ordersBySellers, ref maxOrderNumber, db);
                            }
                            orders.AddRange(ordersBySellers);
                        }
                        db.Orders.AddRange(orders);
                        db.SaveChanges();
                        if (ordersResult.Data.content._meta.currentPage != ordersResult.Data.content._meta.pageCount && ordersResult.Data.content._meta.pageCount != 0)
                        {
                            getOrdersUrl = getOrdersUrl.Replace("page=" + ordersResult.Data.content._meta.currentPage, "page=" + (ordersResult.Data.content._meta.currentPage + 1));
                            await ProcessOrders(getOrdersUrl, authToken, type);
                        }
                        else if (type < 3)
                        {
                            getOrdersUrl = getOrdersUrl.Replace("type=" + type, "type=" + (type + 1)).Replace("page=" + ordersResult.Data.content._meta.currentPage, "page=1");
                            await ProcessOrders(getOrdersUrl, authToken, ++type);
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
            HttpRuntime.Cache["IsRozetkaProcessing"] = false;
        }
    }
}
