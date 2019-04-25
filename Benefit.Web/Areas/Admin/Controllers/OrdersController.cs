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

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.OrdersManagerRoleName + ", " + DomainConstants.AdminRoleName + ", " + DomainConstants.SellerRoleName + ", " + DomainConstants.SellerModeratorRoleName + ", " + DomainConstants.SellerOperatorRoleName)]
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private OrderService OrderService = new OrderService();
        private TransactionsService TransactionsService = new TransactionsService();
        private Logger _logger = LogManager.GetCurrentClassLogger();

        private void UpdateOrderDetails(Order order)
        {
            //update bonuses and points
            var seller = db.Sellers.Include(entry => entry.ShippingMethods).FirstOrDefault(entry => entry.Id == order.SellerId);
            var shipping = seller.ShippingMethods.FirstOrDefault(entry => entry.Name == order.ShippingName);
            order.Sum = order.GetOrderSum();
            order.ShippingCost = (double)(order.Sum < shipping.FreeStartsFrom ? (shipping.CostBeforeFree.HasValue ?  shipping.CostBeforeFree.Value : 0) : 0);
            order.PersonalBonusesSum = order.Sum * seller.UserDiscount / 100;
            order.PointsSum = Double.IsInfinity(order.Sum / SettingsService.DiscountPercentToPointRatio[seller.TotalDiscount]) ? 0 : order.Sum / SettingsService.DiscountPercentToPointRatio[seller.TotalDiscount];
        }

        // GET: /Admin/Orders/
        public ActionResult Index(OrdersFilters ordersFilters, int page = 0)
        {
            var takePerPage = 50;

            var orders =
                db.Orders.Include(o => o.User)
                    .Where(entry => entry.OrderType == ordersFilters.NavigationType);

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
            if (!string.IsNullOrEmpty(ordersFilters.ClientName))
            {
                orders = orders.Where(entry => entry.User.FullName.ToLower().Contains(ordersFilters.ClientName.ToLower()));
            }
            if (!string.IsNullOrEmpty(ordersFilters.PersonnelName))
            {
                orders = orders.Where(entry => entry.PersonnelName.ToLower().Contains(ordersFilters.PersonnelName.ToLower()));
            }
            if (!string.IsNullOrEmpty(ordersFilters.DateRange))
            {
                var dateRangeValues = ordersFilters.DateRange.Split('-');
                var startDate = DateTime.Parse(dateRangeValues.First());
                var endDate = DateTime.Parse(dateRangeValues.Last()).AddTicks(-1).AddDays(1);
                orders = orders.Where(entry => entry.Time >= startDate && entry.Time <= endDate);
            }
            if (ordersFilters.SellerId != null)
            {
                orders = orders.Where(entry => entry.SellerId == ordersFilters.SellerId);
            }
            if (!string.IsNullOrEmpty(ordersFilters.PaymentType))
            {
                orders = orders.Where(entry => ordersFilters.PaymentType.Contains(entry.PaymentType.ToString()));
            }
            else
            {
                ordersFilters.PaymentType = string.Empty;
            }
            if (ordersFilters.NavigationType == OrderType.BenefitSite)
            {
                if (!string.IsNullOrEmpty(ordersFilters.Status))
                {
                    orders = orders.Where(entry => ordersFilters.Status.Contains(entry.Status.ToString()));
                }
                else
                {
                    ordersFilters.Status = string.Empty;
                }
                if (ordersFilters.OrderNumber.HasValue)
                {
                    orders = orders.Where(entry => entry.OrderNumber == ordersFilters.OrderNumber);
                }
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
            ordersFilters.Sum = orders.ToList().Select(l => l.SumWithDiscount).DefaultIfEmpty(0).Sum();
            ordersFilters.Number = orders.Count();
            orders = orders.Skip(page * takePerPage).Take(takePerPage);

            ordersFilters.Orders = new PaginatedList<Order>
            {
                Items = orders.ToList(),
                Pages = ordersTotal / takePerPage + 1,
                ActivePage = page
            };
            ordersFilters.Sorting = (from OrderSortOption sortOption in Enum.GetValues(typeof (OrderSortOption))
                select
                    new SelectListItem()
                    {
                        Text = Enumerations.GetEnumDescription(sortOption),
                        Value = sortOption.ToString(),
                        Selected = sortOption == ordersFilters.Sort
                    }).ToList();
            ordersFilters.Sellers =
                db.Sellers.OrderBy(entry => entry.Name)
                    .Select(entry => new SelectListItem { Text = entry.Name, Value = entry.Id });
            return View(ordersFilters);
        }

        public ActionResult Print(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Include(entry => entry.User).Include(entry => entry.OrderProducts.Select(op=>op.OrderProductOptions)).Include(entry => entry.Transactions).FirstOrDefault(entry => entry.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            order.OrderProducts = order.OrderProducts.OrderBy(entry => entry.Index).ToList();
            return View(order);
        }

        [HttpPost]
        public ActionResult UpdateStatus(OrderStatus orderStatus, string statusComment, string orderId)
        {
            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    var order = db.Orders.Find(orderId);
                    if (order.Status == OrderStatus.Abandoned || order.Status == OrderStatus.Finished)
                    {
                        return Json(new { error = "Статус Замовлення не може бути змінено, будь ласка оновіть сторінку" });
                    }
                    order.Status = orderStatus;

                    var statusStamp = new OrderStatusStamp()
                    {
                        Id = Guid.NewGuid().ToString(),
                        OrderId = orderId,
                        OrderStatus = orderStatus,
                        Time = DateTime.UtcNow,
                        Comment = statusComment,
                        UpdatedBy = HttpUtility.UrlDecode(Request.Cookies[RouteConstants.FullNameCookieName].Value)
                    };
                    db.OrderStatusStamps.Add(statusStamp);

                    //add points and bonuses if order finished and is not bonuses
                    if (orderStatus == OrderStatus.Finished)
                    {
                        TransactionsService.AddOrderFinishedTransaction(order, db);
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
                        var PromotionService = new PromotionService(db);
                        PromotionService.ProcessPromotions(order);
                    }
                    if (orderStatus == OrderStatus.Abandoned && order.PaymentType == PaymentType.Bonuses)
                    {
                        TransactionsService.AddBonusesOrderAbandonedTransaction(order, db);
                    }

                    db.Entry(order).State = EntityState.Modified;
                    db.SaveChanges();
                    dbTransaction.Commit();
                    return Json(new { status = true });
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    _logger.Fatal(ex);
                    return Json(new { error = "Серверна помилка" });
                }
            }
        }

        public ActionResult CheckNewOrder()
        {
            var newOrders =
                db.Orders.Where(entry => entry.OrderType == OrderType.BenefitSite && entry.Status == OrderStatus.Created);
            if (Seller.CurrentAuthorizedSellerId != null)
            {
                newOrders = newOrders.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId);
            }
            return Json(newOrders.Any(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOrdersList(OrderType orderType, int page = 0)
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

        public ActionResult BulkUpdateOrderProducts(string orderId, List<OrderProduct> orderProducts, List<OrderProductOption> orderProductOptions)
        {
            var order = db.Orders.Include(entry => entry.OrderProducts).FirstOrDefault(entry => entry.Id == orderId);
            if (orderProducts != null)
            {
                foreach (var orderProduct in orderProducts)
                {
                    var product = order.OrderProducts.FirstOrDefault(entry => entry.ProductId == orderProduct.ProductId);
                    product.ProductName = orderProduct.ProductName;
                    product.Amount = orderProduct.Amount;
                    product.ProductPrice = orderProduct.ProductPrice;
                    db.Entry(product).State = EntityState.Modified;
                }
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

        public ActionResult DeleteOrderProduct(string orderId, string productId)
        {
            var product =
                db.OrderProducts.FirstOrDefault(entry => entry.OrderId == orderId && entry.ProductId == productId);
            db.OrderProductOptions.RemoveRange(product.DbOrderProductOptions);
            db.OrderProducts.Remove(product);
            var order = db.Orders.Include(entry => entry.OrderProducts.Select(op=>op.OrderProductOptions)).FirstOrDefault(entry => entry.Id == orderId);
            UpdateOrderDetails(order);
            db.SaveChanges();
            return RedirectToAction("Details", new { id = orderId });
        }

        public ActionResult DeleteOrderProductOption(string orderId, string orderProductId, string productOptionId)
        {
            var productOption =
                db.OrderProductOptions.FirstOrDefault(entry => entry.OrderProductId == orderProductId && entry.ProductOptionId == productOptionId);
            db.OrderProductOptions.Remove(productOption);
            db.SaveChanges();
            var order = db.Orders.Include(entry => entry.OrderProducts.Select(op=>op.OrderProductOptions)).FirstOrDefault(entry => entry.Id == orderId);
            UpdateOrderDetails(order);
            db.SaveChanges();
            return RedirectToAction("Details", new { id = orderId });
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
            orderProduct.Id = Guid.NewGuid().ToString();
            orderProduct.ProductId = Guid.NewGuid().ToString();
            db.OrderProducts.Add(orderProduct);
            var order = db.Orders.Include(entry => entry.OrderProducts.Select(op=>op.OrderProductOptions)).FirstOrDefault(entry => entry.Id == orderProduct.OrderId);
            UpdateOrderDetails(order);
            orderProduct.Index = order.OrderProducts.Max(entry => entry.Index) + 1;
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Details", new { id = orderProduct.OrderId });
        }

        // GET: /Admin/Orders/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Include(entry=>entry.Transactions).Include(entry => entry.OrderProducts.Select(op=>op.OrderProductOptions)).Include(entry => entry.OrderStatusStamps).FirstOrDefault(entry => entry.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            order.OrderProducts = order.OrderProducts.OrderBy(entry => entry.Index).ToList();
            var orderSeller = db.Sellers.Find(order.SellerId);
            order.SellerPhone = orderSeller == null ? null : orderSeller.OnlineOrdersPhone;
            return View(order);
        }

        // GET: /Admin/Orders/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Users, "Id", "FullName");
            return View();
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
            var tr = db.Transactions.Include(entry=>entry.Payee).FirstOrDefault(entry=>entry.Id == transactionId);
            if (tr.Type == TransactionType.PersonalSiteBonus)
            {
                tr.Payee.CurrentBonusAccount = tr.Payee.CurrentBonusAccount - tr.Bonuses;
            }
            if (tr.Type == TransactionType.OrderRefund)
            {
                tr.Payee.BonusAccount = tr.Payee.BonusAccount - tr.Bonuses;                
            }
            db.Entry(tr.Payee).State = EntityState.Modified;
            db.Transactions.Remove(tr);
            db.SaveChanges();
            return RedirectToAction("Details", new { id = orderId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
