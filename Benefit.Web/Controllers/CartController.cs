using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using Benefit.Common.Constants;
using Benefit.Common.Extensions;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Services.Cart;
using Benefit.Services.Domain;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;
using Microsoft.AspNet.Identity;
using NLog;

namespace Benefit.Web.Controllers
{
    public class CartController : Controller
    {
        public OrderService OrderService = new OrderService();
        public UserService UserService = new UserService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        [FetchCategories]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CheckEnaughBonuses(double sum)
        {
            var user = UserService.GetUser(User.Identity.GetUserId());
            var result = .0;
            if (user.BonusAccount >= sum)
            {
                result = sum;
            }
            return Json(new
            {
                total = result
            }, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public ActionResult CheckSeller(string sellerId)
        //{
        //    return Json(Cart.CurrentInstance.Order.SellerId != null && Cart.CurrentInstance.Order.SellerId != sellerId, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult AddProduct(OrderProduct product, string sellerId)
        {
            var productsNumber = Cart.CurrentInstance.AddProduct(product, sellerId);
            return Json(productsNumber);
        }

        public ActionResult UpdateQuantity(string productId, string sellerId, double amount)
        {
            var productsNumber = Cart.CurrentInstance.UpdatePoductQuantity(productId, sellerId, amount);
            return Json(productsNumber);
        }

        public ActionResult RemoveProduct(string productId, string sellerId)
        {
            var productsNumber = Cart.CurrentInstance.RemoveProduct(sellerId, productId);
            return Json(productsNumber);
        }

        [HttpPost]
        public ActionResult CompleteOrder(List<OrderProduct> orderProducts, string sellerId)
        {
            Cart.CurrentInstance.ClearSellerOrder(sellerId);
            foreach (var orderProduct in orderProducts)
            {
                Cart.CurrentInstance.AddProduct(orderProduct, sellerId);
            }
            var orderSummary = Cart.CurrentInstance.GetOrderProductsCountAndPrice();
            return Json(new { redirectUrl = Url.Action("order", "cart", new { id = sellerId }), orderSummary.ProductsNumber, orderSummary.Price });
        }

        [FetchCategories]
        public ActionResult Order(string id)
        {
            var model = new CompleteOrderViewModel
            {
                SellerId = id,
                Order = Cart.CurrentInstance.Orders.FirstOrDefault(entry => entry.SellerId == id)
            };
            if (model.Order == null) return HttpNotFound();
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers.Include(entry => entry.Promotions).FirstOrDefault(entry => entry.Id == id);
                var userId = User.Identity.GetUserId();
                if (seller == null) return HttpNotFound();

                //promotions
                var now = DateTime.UtcNow;
                var currentPromotion =
                    seller.Promotions.FirstOrDefault(
                        entry => entry.Start < now && entry.End > now && !entry.IsBonusDiscount && entry.IsActive);

                if (currentPromotion != null)
                {
                    if (currentPromotion.StartTime.HasValue && currentPromotion.EndTime.HasValue)
                    {
                        currentPromotion.StartTimeDt =
                            DateTime.Now.ToLocalTime().StartOfDay().AddHours(currentPromotion.StartTime.Value);
                        currentPromotion.EndTimeDt =
                            DateTime.Now.ToLocalTime().StartOfDay().AddHours(currentPromotion.EndTime.Value);

                        if (DateTime.Now.ToLocalTime() > currentPromotion.StartTimeDt &&
                            DateTime.Now.ToLocalTime() < currentPromotion.EndTimeDt)
                        {
                            model.Order.Sum = model.Order.GetOrderSum();
                            if (model.Order.Sum >= currentPromotion.DiscountFrom)
                            {
                                model.Order.SellerDiscount = currentPromotion.IsValuePercent
                                    ? model.Order.Sum * currentPromotion.DiscountValue / 100
                                    : currentPromotion.DiscountValue;
                                model.Order.SellerDiscountName = currentPromotion.Name;
                            }
                        }
                    }
                    else
                    {
                        model.Order.Sum = model.Order.GetOrderSum();
                        if (model.Order.Sum >= currentPromotion.DiscountFrom)
                        {
                            model.Order.SellerDiscount = currentPromotion.IsValuePercent
                                ? model.Order.Sum * currentPromotion.DiscountValue / 100
                                : currentPromotion.DiscountValue;
                            model.Order.SellerDiscountName = currentPromotion.Name;
                        }
                    }
                }

                model.ShippingMethods = db.ShippingMethods.Where(entry => entry.SellerId == id).ToList();
                model.Addresses = db.Addresses.Include(entry => entry.Region).Where(entry => entry.UserId == userId).ToList();
                if (seller.IsPrePaidPaymentActive)
                    model.PaymentTypes.Add(PaymentType.PrePaid);
                if (seller.IsPostPaidPaymentActive)
                    model.PaymentTypes.Add(PaymentType.PostPaid);
                if (seller.IsCashPaymentActive)
                    model.PaymentTypes.Add(PaymentType.Cash);
                if (seller.IsAcquiringActive)
                    model.PaymentTypes.Add(PaymentType.Acquiring);
                if (seller.IsBonusesPaymentActive)
                    model.PaymentTypes.Add(PaymentType.Bonuses);
            }

            return View(model);
        }

        [FetchCategories]
        public ActionResult OrderCompleted(string number)
        {
            return View(model: number);
        }

        [HttpPost]
        [FetchCategories]
        public ActionResult Order(CompleteOrderViewModel completeOrder)
        {
            completeOrder.Order = Cart.CurrentInstance.Orders.FirstOrDefault(entry => entry.SellerId == completeOrder.SellerId);
            if (completeOrder.Order == null) return HttpNotFound();

            if (completeOrder.PaymentType == PaymentType.Bonuses)
            {
                var sum = Cart.CurrentInstance.GetOrderSum();
                var user = UserService.GetUser(User.Identity.GetUserId());

                if (user.BonusAccount < sum)
                {
                    ModelState.AddModelError("PaymentType", "Недостатньо бонусів на рахунку");
                }
            }
            if (completeOrder.RequireAddress && completeOrder.AddressId == null)
            {
                ModelState.AddModelError("AddressId", "Оберіть адресу доставки");
            }

            if (ModelState.IsValid)
            {
                completeOrder.Order.UserId = User.Identity.GetUserId();
                var orderNumber = OrderService.AddOrder(completeOrder);

                //order notifications
                var NotificationService = new NotificationsService();
                var orderUrl = Url.Action("details", "orders", new { id = completeOrder.Order.Id, area = RouteConstants.AdminAreaName }, Request.Url.Scheme);
                Task.Run(() =>
                    NotificationService.NotifySeller(completeOrder.Order.OrderNumber, orderUrl,
                        completeOrder.Order.SellerId));
                return RedirectToAction("ordercompleted", new { number = orderNumber });
            }
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers.FirstOrDefault(entry => entry.Id == completeOrder.Order.SellerId);
                if (seller == null) return HttpNotFound();
                var userId = User.Identity.GetUserId();
                completeOrder.ShippingMethods = db.ShippingMethods.Where(entry => entry.SellerId == completeOrder.Order.SellerId).ToList();
                completeOrder.Addresses =
                    db.Addresses.Include(entry => entry.Region).Where(entry => entry.UserId == userId).ToList();
                if (seller.IsPrePaidPaymentActive)
                    completeOrder.PaymentTypes.Add(PaymentType.PrePaid);
                if (seller.IsPostPaidPaymentActive)
                    completeOrder.PaymentTypes.Add(PaymentType.PostPaid);
                if (seller.IsCashPaymentActive)
                    completeOrder.PaymentTypes.Add(PaymentType.Cash);
                if (seller.IsAcquiringActive)
                    completeOrder.PaymentTypes.Add(PaymentType.Acquiring);
                if (seller.IsBonusesPaymentActive)
                    completeOrder.PaymentTypes.Add(PaymentType.Bonuses);
            }

            TempData["ErrorMessage"] = ModelState.ModelStateErrors();
            return View(completeOrder);
        }

        public ActionResult GetCart()
        {
            var cart = Cart.CurrentInstance.Orders;
            return PartialView("_CartPartial", cart);
        }
    }
}