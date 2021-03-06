﻿using Benefit.DataTransfer.ApiDto.Allo;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.HttpClient;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Benefit.Services.ExternalApi
{
    public class AlloApiService : IMarketPlaceApi
    {
        private BenefitHttpClient _httpClient = new BenefitHttpClient();
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public string GetAccessToken(string userName, string password)
        {
            var auth = new AuthIngest
            {
                username = userName,
                apiKey = password
            };
            var authUrl = SettingsService.Allo.BaseUrl + "login";
            var postData = JsonConvert.SerializeObject(auth);
            var authResult = _httpClient.Post<AuthDto>(authUrl, postData, "application/json");
            if (authResult.StatusCode == HttpStatusCode.OK)
            {
                return authResult.Data.sessionId;
            }
            return null;
        }

        //public void UpdateOrderStatus(string id, OrderStatus oldStatus, OrderStatus newStatus, string ttn, int tryCount = 1)
        //{
        //    var success = true;
        //    var updateOrderUrl = SettingsService.Rozetka.BaseUrl + "orders/" + id;
        //    var authToken = GetAccessToken(SettingsService.Rozetka.UserName, SettingsService.Rozetka.Password);
        //    if (authToken == null)
        //    {
        //        success = false;
        //    }
        //    else
        //    {
        //        var updateOrderIngest = new UpdateOrderIngest
        //        {
        //            status = (int)newStatus + 1
        //        };
        //        var postData = JsonConvert.SerializeObject(updateOrderIngest);
        //        var ordersResult = _httpClient.Post<BaseDto>(updateOrderUrl, postData, "application/json", authToken, "put");
        //        if (ordersResult.StatusCode == HttpStatusCode.OK)
        //        {
        //            if (!ordersResult.Data.success)
        //            {
        //                success = false;
        //            }
        //            else
        //            {
        //                success = false;
        //            }
        //        }
        //    }
        //    if (!success)
        //    {
        //        if (tryCount <= 3)
        //        {
        //            Thread.Sleep(3000);
        //            UpdateOrderStatus(id, oldStatus, newStatus, ttn, tryCount + 1);
        //        }
        //        else
        //        {
        //            var notificationService = new NotificationsService();
        //            notificationService.NotifyApiFailRequest(id, "Rozetka", oldStatus, newStatus);
        //        }
        //    }
        //}

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

        public async Task ProcessOrders(string getOrdersUrl = null, string authToken = null, int type = 1)
        {
            using (var db = new ApplicationDbContext())
            {
                var ingest = new OrdersIngest()
                {
                    args = new OrdersIngestArgs { limit = 50, offset = 0 }
                };
                //var orderSuffixRegex = new Regex(@"\(\b(\w*.?(bc|BC).+\d\w*)\b\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var orders = new List<Order>();
                getOrdersUrl = SettingsService.Allo.BaseUrl + "call?apiPath=orders.orderList";
                var lastOrder = db.Orders.Where(entry => entry.OrderType == OrderType.Allo).OrderByDescending(entry => entry.Time).FirstOrDefault();
                if (lastOrder != null)
                {
                    ingest.args.accepted_from = lastOrder.Time.ToString("yyyy-MM-dd hh:mm:ss");
                }
                else
                {
                    ingest.args.accepted_from = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd hh:mm:ss");
                }
                authToken = GetAccessToken(SettingsService.Allo.UserName, SettingsService.Allo.ApiKey);
                if (authToken != null)
                {
                    ingest.sessionId = authToken;
                    var postData = JsonConvert.SerializeObject(ingest);
                    var ordersResult = _httpClient.Post<OrdersDto>(getOrdersUrl, postData, "application/json");
                    if (ordersResult.StatusCode == HttpStatusCode.OK)
                    {
                        var maxOrderNumber = db.Orders.Max(entry => entry.OrderNumber);
                        var rOrders = ordersResult.Data.orders.Where(entry => !db.Orders.Any(or => or.ExternalId == entry.id)).ToList();
                        foreach (var rOrder in rOrders)
                        {
                            //var productNames = rOrder.purchases.Select(entry => orderSuffixRegex.Match(entry.item_name).Value.ToLower()).ToList();
                            //var products = db.Products.Where(entry => productNames.Any(pn => entry.Name.ToLower().Contains(pn))).ToList();
                            //var sellerIds = products.Select(entry => entry.SellerId).Distinct().ToList();
                            //foreach (var sellerId in sellerIds)
                            //{
                            var order = new Order()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ExternalId = rOrder.id,
                                OrderType = OrderType.Allo,
                                Description = rOrder.payment_type,
                                //SellerId = sellerId,
                                OrderNumber = ++maxOrderNumber,
                                Time = DateTime.Parse(rOrder.created_date),
                                //Status = ((OrderStatus)rOrder.status - 1),
                                Status = OrderStatus.Created,
                                ShippingName = rOrder.shipping.type,
                                ShippingTrackingNumber = rOrder.shipping.tracking_number,
                                PersonalBonusesSum = 0,
                                PointsSum = 0,
                                LastModified = DateTime.UtcNow,
                                UserName = string.Format("{0} {1}", rOrder.customer.firstname, rOrder.customer.lastname),
                                UserPhone = rOrder.customer.telephone
                            };
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
                                    order.PaymentType = PaymentType.PrePaid;
                                    break;
                                default:
                                    order.PaymentType = PaymentType.Cash;
                                    break;
                            }
                            //foreach (var rProduct in rOrder.products)
                            //{
                            //    var product = products.FirstOrDefault(entry => entry.Name == rProduct.item_name);
                            //    if (product != null)
                            //    {
                            //        var image = product.Images.OrderBy(entry => entry.Order).FirstOrDefault();
                            //        var orderProduct = new OrderProduct()
                            //        {
                            //            Id = Guid.NewGuid().ToString(),
                            //            ExternalId = rProduct.id,
                            //            OrderId = order.Id,
                            //            ProductName = rProduct.item_name,
                            //            ProductId = product.Id,
                            //            ProductSku = product.SKU,
                            //            ProductPrice = rProduct.price,
                            //            Amount = rProduct.quantity,
                            //            ProductImageUrl = image == null ? null : image.ImageUrl
                            //        };
                            //        order.OrderProducts.Add(orderProduct);
                            //    }
                            //}
                            order.Sum = order.GetOrderSum();
                            orders.Add(order);
                            //}
                        }
                        db.Orders.AddRange(orders);
                        db.SaveChanges();
                        //if (ordersResult.Data.content._meta.currentPage != ordersResult.Data.content._meta.pageCount && ordersResult.Data.content._meta.pageCount != 0)
                        //{
                        //    getOrdersUrl = getOrdersUrl.Replace("page=" + ordersResult.Data.content._meta.currentPage, "page=" + (ordersResult.Data.content._meta.currentPage + 1));
                        //    ProcessOrders(getOrdersUrl, authToken);
                        //}
                        //else if (type < 3)
                        //{
                        //    getOrdersUrl = getOrdersUrl.Replace("type=" + type, "type=" + (type + 1));
                        //    ProcessOrders(getOrdersUrl, authToken, ++type);
                        //}
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

        public void UpdateOrderStatus(string id, OrderStatus oldStatus, OrderStatus newStatus, string ttn, int tryCount, string sellerComment)
        {
            throw new NotImplementedException();
        }
    }
}
