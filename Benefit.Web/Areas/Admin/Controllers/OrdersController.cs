using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models.Enums;
using Benefit.Services;
using Benefit.Services.Domain;
using Benefit.Web.Helpers;
using Benefit.Web.Models.Admin;
using NLog;
using Benefit.Common.Helpers;
using Benefit.Services.ExternalApi;
using Benefit.Common.Extensions;
using Benefit.DataTransfer.JSON;
using System.Threading.Tasks;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.OrdersManagerRoleName + ", " + DomainConstants.AdminRoleName + ", " + DomainConstants.SellerRoleName + ", " + DomainConstants.SellerModeratorRoleName + ", " + DomainConstants.SellerOperatorRoleName)]
    public class OrdersController : Controller
    {
        private OrderService OrderService = new OrderService();
        private TransactionsService TransactionsService = new TransactionsService();
        private Logger _logger = LogManager.GetCurrentClassLogger();

        private void UpdateOrderDetails(Order order)
        {
            using (var db = new ApplicationDbContext())
            {
                //update bonuses and points
                var seller = db.Sellers.Include(entry => entry.ShippingMethods).FirstOrDefault(entry => entry.Id == order.SellerId);
                var shipping = seller.ShippingMethods.FirstOrDefault(entry => entry.Name == order.ShippingName);
                order.Sum = order.GetOrderSum();
                order.ShippingCost = (double)(order.Sum < shipping.FreeStartsFrom ? (shipping.CostBeforeFree.HasValue ? shipping.CostBeforeFree.Value : 0) : 0);
                order.PersonalBonusesSum = order.Sum * seller.UserDiscount / 100;
                order.PointsSum = Double.IsInfinity(order.Sum / SettingsService.DiscountPercentToPointRatio[seller.TotalDiscount]) ? 0 : order.Sum / SettingsService.DiscountPercentToPointRatio[seller.TotalDiscount];
            }
        }
        public ActionResult SearchProduct(string query, string sellerId)
        {
            SearchService SearchService = new SearchService();

            var products = SearchService.SearchProducts(query, Seller.CurrentAuthorizedSellerId);
            var pResult = products.Select(entry => new
            {
                id = entry.Id,
                name = entry.Name,
                sku = entry.SKU,
                price = entry.Price,
                isWeight = entry.IsWeightProduct,
                image = entry.Images.FirstOrDefault() == null ? null : entry.Images.FirstOrDefault().IsAbsoluteUrl ? entry.Images.FirstOrDefault().ImageUrl : string.Format("~/Images/ProductGallery/{0}/{1}", entry.Id, entry.Images.FirstOrDefault())
            }).ToList();
            var result = new AutocompleteSearch
            {
                query = query,
                suggestions = pResult.Select(entry => new ValueData()
                {
                    value = entry.id,
                    data = new { entry.name, entry.image, entry.sku, entry.price, entry.isWeight }
                }).ToArray()
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: /Admin/Orders/
        public ActionResult Index(AdminOrdersFilters ordersFilters, int page = 0)
        {
            using (var db = new ApplicationDbContext())
            {
                var takePerPage = 16;

                var orders =
                    db.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderStatusStamps)
                    .Include(o => o.OrderProducts);

                if (Seller.CurrentAuthorizedSellerId != null)
                {
                    orders = orders.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId);
                    var seller = db.Sellers.Find(Seller.CurrentAuthorizedSellerId);
                    if (seller.RepeatingTransactionInterval != null)
                    {
                        orders.Where(
                            entry =>
                                orders.Any(
                                    o => o.Id != entry.Id &&
                                        o.UserId == entry.UserId &&
                                         DbFunctions.DiffHours(o.Time, entry.Time) < seller.RepeatingTransactionInterval)).ToList().ForEach(entry => entry.IsRepeating = true);
                    }
                }

                if (ordersFilters.Status == null)
                {
                    ordersFilters.Status = 0;
                }
                switch (ordersFilters.Status)
                {
                    case 0:
                        orders = orders.Where(entry => entry.Status == OrderStatus.Created ||
                                                       entry.Status == OrderStatus.PassedToDelivery ||
                                                       entry.Status == OrderStatus.Processed ||
                                                       entry.Status == OrderStatus.AwaitingDelivery ||
                                                       entry.Status == OrderStatus.IsDelivering ||
                                                       entry.Status == OrderStatus.AwaitingDelivery ||
                                                       entry.Status == OrderStatus.ContactFail1 ||
                                                       entry.Status == OrderStatus.ContactFail2 ||
                                                       entry.Status == OrderStatus.WaitingInSelfPickup);
                        break;
                    case 1:
                        orders = orders.Where(entry => entry.Status == OrderStatus.Finished);
                        break;
                    case 2:
                        orders = orders.Where(entry => entry.Status != OrderStatus.Created &&
                                                       entry.Status != OrderStatus.PassedToDelivery &&
                                                       entry.Status != OrderStatus.Processed &&
                                                       entry.Status != OrderStatus.AwaitingDelivery &&
                                                       entry.Status != OrderStatus.IsDelivering &&
                                                       entry.Status != OrderStatus.Finished &&
                                                       entry.Status != OrderStatus.AwaitingDelivery &&
                                                       entry.Status != OrderStatus.ContactFail1 &&
                                                       entry.Status != OrderStatus.ContactFail2 &&
                                                       entry.Status != OrderStatus.WaitingInSelfPickup);
                        break;
                }

                if (ordersFilters.OrderNumber.HasValue)
                {
                    orders = orders.Where(entry => entry.OrderNumber == ordersFilters.OrderNumber);
                }
                if (!string.IsNullOrEmpty(ordersFilters.ProductName))
                {
                    orders = orders.Where(entry => entry.OrderProducts.Select(op => op.ProductName).Any(pn => pn.ToLower().Contains(ordersFilters.ProductName.ToLower())) ||
                    entry.OrderProducts.Select(op => op.ProductSku.ToString()).Contains(ordersFilters.ProductName));
                }
                if (!string.IsNullOrEmpty(ordersFilters.DateRange))
                {
                    var dateRangeValues = ordersFilters.DateRange.Split('-');
                    var startDate = DateTime.Parse(dateRangeValues.First());
                    var endDate = DateTime.Parse(dateRangeValues.Last()).AddTicks(-1).AddDays(1);
                    orders = orders.Where(entry => entry.Time >= startDate && entry.Time <= endDate);
                }

                if (!string.IsNullOrEmpty(ordersFilters.Phone))
                {
                    orders = orders.Where(entry => entry.User.PhoneNumber.Contains(ordersFilters.Phone));
                }
                if (!string.IsNullOrEmpty(ordersFilters.ClientName))
                {
                    orders = orders.Where(entry => entry.User.FullName.ToLower().Contains(ordersFilters.ClientName.ToLower()));
                }
                if (!string.IsNullOrEmpty(ordersFilters.Comment))
                {
                    orders = orders.Where(entry => entry.OrderStatusStamps.Select(s => s.Comment).Any(st => st.ToLower().Contains(ordersFilters.Comment.ToLower())));
                }
                if (ordersFilters.SellerId != null)
                {
                    orders = orders.Where(entry => entry.SellerId == ordersFilters.SellerId);
                }

                if (ordersFilters.ClientGrouping)
                {
                    var groupedOrders =
                        orders.GroupBy(entry => entry.UserId).OrderByDescending(entry => entry.Count()).ToList();
                    orders = groupedOrders.SelectMany(entry => entry.OrderByDescending(grp => grp.Time)).AsQueryable();
                }
                else
                {
                    switch (ordersFilters.Sort)
                    {
                        case OrderSortOption.DateAsc:
                            orders = orders.OrderBy(entry => entry.Time);
                            break;
                        case OrderSortOption.DateDesc:
                            orders = orders.OrderByDescending(entry => entry.Time);
                            break;
                        case OrderSortOption.SumAsc:
                            orders = orders.OrderBy(entry => entry.Sum);
                            break;
                        case OrderSortOption.SumDesc:
                            orders = orders.OrderByDescending(entry => entry.Sum);
                            break;
                    }
                }
                var ordersTotal = orders.Count();
                ordersFilters.Number = orders.Count();
                orders = orders.Skip(page * takePerPage).Take(takePerPage);
                ordersFilters.Orders = new PaginatedList<Order>
                {
                    Items = orders.ToList(),
                    Pages = ordersTotal / takePerPage + 1,
                    ActivePage = page
                };
                var productIdsInOrders = ordersFilters.Orders.Items.SelectMany(entry => entry.OrderProducts).Select(entry => entry.ProductId).Distinct().ToList();
                var productsInOrders = db.Products.Include(entry => entry.Images).Where(entry => productIdsInOrders.Contains(entry.Id)).ToList();
                var sellerIds = ordersFilters.Orders.Items.Select(entry => entry.SellerId).Distinct().ToList();
                var allOrderSellers = db.Sellers.Where(entry => sellerIds.Contains(entry.Id)).ToList();
                ordersFilters.Orders.Items.ForEach(order =>
                {
                    if (order.User == null)
                    {
                        order.User = new ApplicationUser
                        {
                            FullName = order.UserName,
                            PhoneNumber = order.UserPhone
                        };
                    }
                    var orderSeller = allOrderSellers.FirstOrDefault(entry => entry.Id == order.SellerId);
                    order.SellerPhone = orderSeller == null ? null : orderSeller.OnlineOrdersPhone;
                    order.SellerName = orderSeller == null ? null : orderSeller.Name;
                    foreach (var orderProduct in order.OrderProducts)
                    {
                        var product = productsInOrders.FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                        if (product != null)
                        {
                            orderProduct.ProductSku = product.SKU;
                            orderProduct.IsWeightProduct = product.IsWeightProduct;
                            var productImg = product.Images.FirstOrDefault();
                            if (productImg != null)
                            {
                                if (productImg.IsAbsoluteUrl)
                                {
                                    orderProduct.ProductImageUrl = productImg.ImageUrl;
                                }
                                else
                                {
                                    orderProduct.ProductImageUrl = string.Format("/Images/ProductGallery/{0}/{1}", product.Id, productImg.ImageUrl);
                                }
                            }
                        }
                    }
                });
                ordersFilters.Sorting = (from OrderSortOption sortOption in Enum.GetValues(typeof(OrderSortOption))
                                         select
                                             new SelectListItem()
                                             {
                                                 Text = Enumerations.GetEnumDescription(sortOption),
                                                 Value = sortOption.ToString(),
                                                 Selected = sortOption == ordersFilters.Sort
                                             }).ToList();
                ordersFilters.Sellers =
                    db.Sellers.OrderBy(entry => entry.Name)
                        .Select(entry => new SelectListItem { Text = entry.Name, Value = entry.Id }).ToList();
                return View(ordersFilters);
            }
        }

        public ActionResult GetOrderPartial(string id, bool expanded = true)
        {
            using (var db = new ApplicationDbContext())
            {
                var order = db.Orders
                .Include(entry => entry.OrderStatusStamps)
                .Include(entry => entry.OrderProducts)
                .FirstOrDefault(entry => entry.OrderNumber.ToString() == id || entry.Id == id);
                var orderSeller = db.Sellers.Find(order.SellerId);
                order.SellerPhone = orderSeller == null ? null : orderSeller.OnlineOrdersPhone;
                var productIdsInOrders = order.OrderProducts.Select(entry => entry.ProductId).Distinct().ToList();
                var productsInOrders = db.Products.Include(entry => entry.Images).Where(entry => productIdsInOrders.Contains(entry.Id)).ToList();
                foreach (var orderProduct in order.OrderProducts)
                {
                    var product = productsInOrders.FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                    if (product != null)
                    {
                        orderProduct.ProductSku = product.SKU;
                        orderProduct.IsWeightProduct = product.IsWeightProduct;
                        var productImg = product.Images.FirstOrDefault();
                        if (productImg != null)
                        {
                            if (productImg.IsAbsoluteUrl)
                            {
                                orderProduct.ProductImageUrl = productImg.ImageUrl;
                            }
                            else
                            {
                                orderProduct.ProductImageUrl = string.Format("~/Images/ProductGallery/{0}/{1}", product.Id, productImg.ImageUrl);
                            }
                        }
                    }
                }
                ViewBag.ExternalRequest = expanded;
                return PartialView("_OrderPartial", order);
            }
        }

        public ActionResult GetStatus(string orderId)
        {
            using (var db = new ApplicationDbContext())
            {
                var order = db.Orders.Find(orderId);
                return PartialView("_OrderStatusPartial", order);
            }
        }

        public ActionResult GetProcessed()
        {
            return View();
        }

        public ActionResult Print(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var order = db.Orders.Include(entry => entry.User).Include(entry => entry.OrderProducts.Select(op => op.OrderProductOptions)).Include(entry => entry.Transactions).FirstOrDefault(entry => entry.Id == id);
                if (order == null)
                {
                    return HttpNotFound();
                }
                order.OrderProducts = order.OrderProducts.OrderBy(entry => entry.Index).ToList();
                return View(order);
            }
        }

        [HttpPost]
        public ActionResult AddComment(string orderId, OrderStatus status, string comment)
        {
            using (var db = new ApplicationDbContext())
            {
                var statusStamp = new StatusStamp()
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderId = orderId,
                    Status = (int)status,
                    Time = DateTime.UtcNow,
                    Comment = comment,
                    UpdatedBy = HttpUtility.UrlDecode(Request.Cookies[RouteConstants.FullNameCookieName].Value)
                };
                db.StatusStamps.Add(statusStamp);
                db.SaveChanges();
                var rozetkaService = new RozetkaApiService();
                var order = db.Orders.Include(entry => entry.OrderStatusStamps).FirstOrDefault(entry => entry.Id == orderId);
                if (order.OrderType == OrderType.Rozetka)
                {
                    Task.Run(() =>
                       rozetkaService.UpdateOrderStatus(order.ExternalId, order.Status, status, order.ShippingTrackingNumber, sellerComment: comment)
                    );
                }
                var partialHtml = ControllerContext.RenderPartialToString("_OrderStatusPartial", order);
                return Json(new { statusPartial = partialHtml });
            }
        }
        [HttpPost]
        public ActionResult UpdateStatus(OrderStatus orderStatus, string statusComment, string orderId, string delieveryType, string delieveryTracking, string delieveryAddress)
        {
            using (var db = new ApplicationDbContext())
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var order = db.Orders.Find(orderId);
                        var oldStatus = order.Status;
                        order.Status = orderStatus;
                        if (delieveryType != null)
                        {
                            order.ShippingName = delieveryType;
                        }
                        if (delieveryTracking != null)
                        {
                            order.ShippingTrackingNumber = delieveryTracking;
                        }
                        if (delieveryAddress != null)
                        {
                            order.ShippingAddress = delieveryAddress;
                        }

                        var statusStamp = new StatusStamp()
                        {
                            Id = Guid.NewGuid().ToString(),
                            OrderId = orderId,
                            Status = (int)orderStatus,
                            Time = DateTime.UtcNow,
                            Comment = statusComment,
                            UpdatedBy = HttpUtility.UrlDecode(Request.Cookies[RouteConstants.FullNameCookieName].Value)
                        };
                        db.StatusStamps.Add(statusStamp);

                        if (orderStatus == OrderStatus.Abandoned)
                        {
                            if (order.ExternalId != null && order.OrderType == OrderType.Rozetka)
                            {
                                var rozetkaService = new RozetkaApiService();
                                rozetkaService.RemoveOrderPurchases(order);
                            }
                        }
                        else if (orderStatus == OrderStatus.PassedToDelivery)
                        {
                            var smsService = new SmsService();
                            var phone = order.UserPhone.ToPhoneFormat();
                            smsService.Send(phone, string.Format(SmsService.npTtnSmsFormat, order.ShippingAddress.Translit(), order.ShippingTrackingNumber, order.Sum));
                        }
                        //add points and bonuses if order finished and is not bonuses
                        else if (orderStatus == OrderStatus.Finished)
                        {
                            if (order.OrderType == OrderType.Rozetka && order.ExternalId != null)
                            {
                                var rozetkaOrders = db.Orders.Where(entry => entry.ExternalId == order.ExternalId).ToList();
                                if (rozetkaOrders.All(entry => entry.Status == OrderStatus.Finished || entry.Status == OrderStatus.Abandoned))
                                {
                                    var marketplaceService = BaseMarketPlaceApi.GetMarketplaceServiceInstance(order.OrderType);
                                    Task.Run(() => marketplaceService.UpdateOrderStatus(order.ExternalId, oldStatus, orderStatus, null, sellerComment: statusComment));
                                }
                            }
                            TransactionsService.AddOrderFinishedTransaction(order, db);
                            TransactionsService.AddOrderFinishedSellerTransaction(order, db);
                            //update available amount
                            foreach (var orderProduct in order.OrderProducts)
                            {
                                var product = db.Products.Find(orderProduct.ProductId);
                                if (product == null) continue;
                                if (product.AvailableAmount != null && product.AvailableAmount > 0)
                                {
                                    product.AvailableAmount = product.AvailableAmount - (int)orderProduct.Amount;
                                    if (product.AvailabilityState == ProductAvailabilityState.Available && product.AvailableAmount <= ProductConstants.EndingNumber)
                                    {
                                        product.AvailabilityState = ProductAvailabilityState.Ending;
                                    }
                                }
                                db.Entry(product).State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            if (order.ExternalId != null)
                            {
                                var marketplaceService = BaseMarketPlaceApi.GetMarketplaceServiceInstance(order.OrderType);
                                Task.Run(() => marketplaceService.UpdateOrderStatus(order.ExternalId, oldStatus, orderStatus, null, sellerComment: statusComment));
                            }
                        }
                        if (orderStatus == OrderStatus.Abandoned ||
                            orderStatus == OrderStatus.NotProcessedBySeller ||
                            orderStatus == OrderStatus.OverduedDelivery ||
                            orderStatus == OrderStatus.PackageNotAquired ||
                            orderStatus == OrderStatus.RefusedFromProducts ||
                            orderStatus == OrderStatus.Defect ||
                            orderStatus == OrderStatus.UnsuitedPayment ||
                            orderStatus == OrderStatus.NoncontactCustomer ||
                            orderStatus == OrderStatus.Returning ||
                            orderStatus == OrderStatus.UnacceptableProduct ||
                            orderStatus == OrderStatus.UnacceptableShipping ||
                            orderStatus == OrderStatus.WrongContactInfo ||
                            orderStatus == OrderStatus.WrongSitePrice ||
                            orderStatus == OrderStatus.ReserveTimeOver ||
                            orderStatus == OrderStatus.OrderRestored ||
                            orderStatus == OrderStatus.UnacceptableOrderGrouping ||
                            orderStatus == OrderStatus.UnacceptableShippingPrice ||
                            orderStatus == OrderStatus.UnacceptableShippingTime ||
                            orderStatus == OrderStatus.UnacceptableNoncashPayment ||
                            orderStatus == OrderStatus.UnacceptablePrePayment ||
                            orderStatus == OrderStatus.UnacceptableProductQuality ||
                            orderStatus == OrderStatus.UnacceptableProductOptions ||
                            orderStatus == OrderStatus.CustomerRefused ||
                            orderStatus == OrderStatus.AnotherSiteBought ||
                            orderStatus == OrderStatus.NotAvailable ||
                            orderStatus == OrderStatus.Fake ||
                            orderStatus == OrderStatus.CustomerAbolished ||
                            orderStatus == OrderStatus.Test)
                        {
                            TransactionsService.AddOrderAbandonedSellerTransaction(order, db);
                            if (order.PaymentType == PaymentType.Bonuses)
                            {
                                TransactionsService.AddBonusesOrderAbandonedTransaction(order, db);
                            }
                        }

                        db.Entry(order).State = EntityState.Modified;
                        db.SaveChanges();
                        dbTransaction.Commit();
                        var partialHtml = ControllerContext.RenderPartialToString("_OrderStatusPartial", order);
                        var statusPreviewHtml = ControllerContext.RenderPartialToString("_OrderStatusPreviewPartial", order);
                        return Json(new { statusPartial = partialHtml, statusPreview = statusPreviewHtml });
                    }
                    catch (Exception ex)
                    {
                        dbTransaction.Rollback();
                        _logger.Fatal(ex);
                        return Json(new { error = "Серверна помилка" });
                    }
                }
            }
        }

        public ActionResult CheckNewOrder(DateTime? time)
        {
            using (var db = new ApplicationDbContext())
            {
                var newOrders =
                db.Orders.Where(entry => entry.OrderType == OrderType.BenefitSite && entry.Status == OrderStatus.Created);
                if (Seller.CurrentAuthorizedSellerId != null)
                {
                    newOrders = newOrders.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId);
                }
                if (time.HasValue)
                {
                    newOrders = newOrders.Where(entry => entry.Time > time);
                }
                var orderIds = newOrders.Select(entry => entry.Id).ToList();
                return Json(orderIds, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetOrdersList(OrderType orderType, int page = 0)
        {
            using (var db = new ApplicationDbContext())
            {
                var takePerPage = 50;

                var orders =
                    db.Orders.Include(o => o.User)
                        .OrderByDescending(entry => entry.Time)
                        .Where(entry => entry.OrderType == orderType);

                if (Seller.CurrentAuthorizedSellerId != null)
                {
                    orders = orders.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId);
                }
                var ordersTotal = orders.Count();
                page = page - 1;
                orders = orders.Skip(page * takePerPage).Take(takePerPage);
                var ordersHtml = ControllerContext.RenderPartialToString("_OrdersListPartial", new PaginatedList<Order>
                {
                    Items = orders.ToList(),
                    Pages = ordersTotal / takePerPage + 1,
                    ActivePage = page
                });
                return Json(new
                {
                    html = ordersHtml,
                    hasNewOrder = db.Orders.Any(entry => entry.Status == OrderStatus.Created)
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult BulkUpdateOrderProducts(string orderId, List<OrderProduct> orderProducts, List<OrderProductOption> orderProductOptions)
        {
            using (var db = new ApplicationDbContext())
            {
                var order = db.Orders.Include(entry => entry.OrderProducts).FirstOrDefault(entry => entry.Id == orderId);
                if (orderProducts != null)
                {
                    foreach (var orderProduct in orderProducts)
                    {
                        var product = order.OrderProducts.FirstOrDefault(entry => entry.ProductId == orderProduct.ProductId);
                        if (product == null)
                        {
                            var pr = db.Products.FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                            if (pr != null)
                            {
                                product = new OrderProduct
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    ProductId = orderProduct.ProductId,
                                    ProductName = pr.Name,
                                    OrderId = order.Id,
                                    ProductSku = pr.SKU,
                                    ProductPrice = pr.Price,
                                    Amount = orderProduct.Amount
                                };
                                db.OrderProducts.Add(product);
                            }
                        }
                        else
                        {
                            product.Amount = orderProduct.Amount;
                            db.Entry(product).State = EntityState.Modified;
                        }
                    }
                    var productIds = orderProducts.Select(entry => entry.ProductId).ToList();
                    var existingProductIds = order.OrderProducts.Select(entry => entry.ProductId).ToList();
                    var productIdsToDelete = existingProductIds.Except(productIds).ToList();
                    db.OrderProducts.RemoveRange(db.OrderProducts.Where(entry => entry.OrderId == orderId && productIdsToDelete.Contains(entry.ProductId)));
                }
                if (orderProductOptions != null)
                {
                    var productOptions = order.OrderProducts.SelectMany(entry => entry.OrderProductOptions).ToList();
                    foreach (var orderProductOption in orderProductOptions)
                    {
                        var productOption =
                            productOptions.FirstOrDefault(
                                entry =>
                                    entry.ProductOptionId == orderProductOption.ProductOptionId &&
                                    entry.OrderProductId == orderProductOption.OrderProductId);
                        productOption.ProductOptionName = orderProductOption.ProductOptionName;
                        productOption.Amount = orderProductOption.Amount;
                        productOption.ProductOptionPriceGrowth = orderProductOption.ProductOptionPriceGrowth;
                        db.Entry(productOption).State = EntityState.Modified;
                    }
                }
                //save products and options
                db.SaveChanges();

                //update order itself
                UpdateOrderDetails(order);
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return Json(Url.Action("Details", new { id = orderId }));
            }
        }

        public ActionResult DeleteOrderProduct(string orderId, string productId)
        {
            using (var db = new ApplicationDbContext())
            {
                var product =
                    db.OrderProducts.FirstOrDefault(entry => entry.OrderId == orderId && entry.ProductId == productId);
                db.OrderProductOptions.RemoveRange(product.DbOrderProductOptions);
                db.OrderProducts.Remove(product);
                var order = db.Orders.Include(entry => entry.OrderProducts.Select(op => op.OrderProductOptions)).FirstOrDefault(entry => entry.Id == orderId);
                UpdateOrderDetails(order);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = orderId });
            }
        }

        public ActionResult DeleteOrderProductOption(string orderId, string orderProductId, string productOptionId)
        {
            using (var db = new ApplicationDbContext())
            {
                var productOption =
                db.OrderProductOptions.FirstOrDefault(entry => entry.OrderProductId == orderProductId && entry.ProductOptionId == productOptionId);
                db.OrderProductOptions.Remove(productOption);
                db.SaveChanges();
                var order = db.Orders.Include(entry => entry.OrderProducts.Select(op => op.OrderProductOptions)).FirstOrDefault(entry => entry.Id == orderId);
                UpdateOrderDetails(order);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = orderId });
            }
        }

        public ActionResult AddProductForm(string orderId)
        {
            return PartialView("_OrderProductForm", new OrderProduct()
            {
                OrderId = orderId
            });
        }

        [HttpPost]
        public ActionResult AddOrderProduct(OrderProduct orderProduct)
        {
            using (var db = new ApplicationDbContext())
            {
                orderProduct.Id = Guid.NewGuid().ToString();
                orderProduct.ProductId = Guid.NewGuid().ToString();
                db.OrderProducts.Add(orderProduct);
                var order = db.Orders.Include(entry => entry.OrderProducts.Select(op => op.OrderProductOptions)).FirstOrDefault(entry => entry.Id == orderProduct.OrderId);
                UpdateOrderDetails(order);
                orderProduct.Index = order.OrderProducts.Max(entry => entry.Index) + 1;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = orderProduct.OrderId });
            }
        }

        public ActionResult GetEditForm(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var order = db.Orders.Include(entry => entry.OrderProducts).FirstOrDefault(entry => entry.Id == id);
                var productIdsInOrders = order.OrderProducts.Select(entry => entry.ProductId).Distinct().ToList();
                var productsInOrders = db.Products.Include(entry => entry.Images).Where(entry => productIdsInOrders.Contains(entry.Id)).ToList();
                foreach (var orderProduct in order.OrderProducts)
                {
                    var product = productsInOrders.FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                    if (product != null)
                    {
                        orderProduct.ProductSku = product.SKU;
                        orderProduct.IsWeightProduct = product.IsWeightProduct;
                        var productImg = product.Images.FirstOrDefault();
                        if (productImg != null)
                        {
                            if (productImg.IsAbsoluteUrl)
                            {
                                orderProduct.ProductImageUrl = productImg.ImageUrl;
                            }
                            else
                            {
                                orderProduct.ProductImageUrl = string.Format("~/Images/ProductGallery/{0}/{1}", product.Id, productImg.ImageUrl);
                            }
                        }
                    }
                }
                return PartialView("_EditPartial", order);
            }
        }
        // GET: /Admin/Orders/Details/5
        public ActionResult Details(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var order = db.Orders
                    .Include(entry => entry.User)
                    .Include(entry => entry.Transactions)
                    .Include(entry => entry.OrderProducts.Select(op => op.OrderProductOptions))
                    .Include(entry => entry.OrderStatusStamps).FirstOrDefault(entry => entry.Id == id);
                if (order == null)
                {
                    return HttpNotFound();
                }
                order.OrderProducts = order.OrderProducts.OrderBy(entry => entry.Index).ToList();
                var productIds = order.OrderProducts.Select(entry => entry.ProductId).ToList();
                var products = db.Products.Where(entry => productIds.Contains(entry.Id)).ToList();
                foreach (var orderProduct in order.OrderProducts)
                {
                    var product = products.FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                    if (product != null)
                    {
                        orderProduct.ProductSku = product.SKU;
                        orderProduct.ProductUrlName = product.UrlName;
                    }
                }
                var orderSeller = db.Sellers.Find(order.SellerId);
                order.SellerPhone = orderSeller == null ? null : orderSeller.OnlineOrdersPhone;
                return View(order);
            }
        }

        // GET: /Admin/Orders/Create
        public ActionResult Create()
        {
            using (var db = new ApplicationDbContext())
            {
                ViewBag.UserId = new SelectList(db.Users, "Id", "FullName");
                return View();
            }
        }

        [Authorize(Roles = DomainConstants.SuperAdminRoleName)]
        public ActionResult Delete(string id)
        {
            OrderService.DeleteOrder(id);
            return Json(new { status = true });
        }

        [Authorize(Roles = DomainConstants.AdminRoleName)]
        public ActionResult DeleteOrderTransaction(string orderId, string transactionId)
        {
            using (var db = new ApplicationDbContext())
            {
                var tr = db.Transactions.Include(entry => entry.Payee).FirstOrDefault(entry => entry.Id == transactionId);
                if (tr.Type == TransactionType.CashbackBonus || tr.Type == TransactionType.OrderRefund)
                {
                    tr.Payee.BonusAccount = tr.Payee.BonusAccount - tr.Bonuses;
                }
                db.Entry(tr.Payee).State = EntityState.Modified;
                db.Transactions.Remove(tr);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = orderId });
            }
        }
    }
}
