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
using Microsoft.AspNet.Identity;
using NLog;

namespace Benefit.Web.Controllers
{
    public class CartController : Controller
    {
        public OrderService OrderService = new OrderService();
        public UserService UserService = new UserService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

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

        [HttpGet]
        public ActionResult CheckSeller(string sellerId)
        {
            return Json(Cart.CurrentInstance.Order.SellerId != null && Cart.CurrentInstance.Order.SellerId != sellerId, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddProduct(OrderProduct product, string sellerId)
        {
            var productsNumber = Cart.CurrentInstance.AddProduct(product, sellerId);
            return Json(productsNumber);
        }

        public ActionResult RemoveProduct(string productId)
        {
            var productsNumber = Cart.CurrentInstance.RemoveProduct(productId);
            return Json(productsNumber);
        }

        [HttpPost]
        public ActionResult CompleteOrder(List<OrderProduct> orderProducts, string sellerId)
        {
            Cart.CurrentInstance.Clear();
            foreach (var orderProduct in orderProducts)
            {
                Cart.CurrentInstance.AddProduct(orderProduct, sellerId);
            }
            var orderSummary = Cart.CurrentInstance.GetOrderProductsCountAndPrice();
            return Json(new { redirectUrl = Url.Action("Order"), orderSummary.ProductsNumber, orderSummary.Price });
        }

        [Authorize]
        public ActionResult Order()
        {
            var model = new CompleteOrderViewModel();
            model.Order = Cart.CurrentInstance.Order;
            var sellerId = Cart.CurrentInstance.Order.SellerId;
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers.Include(entry => entry.Promotions).FirstOrDefault(entry => entry.Id == sellerId);
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
                                    ? model.Order.Sum*currentPromotion.DiscountValue/100
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
                                ? model.Order.Sum*currentPromotion.DiscountValue/100
                                : currentPromotion.DiscountValue;
                            model.Order.SellerDiscountName = currentPromotion.Name;
                        }
                    }
                }

                model.ShippingMethods = db.ShippingMethods.Where(entry => entry.SellerId == sellerId).ToList();
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

        public ActionResult OrderCompleted(string number)
        {
            return View(model: number);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Order(CompleteOrderViewModel completeOrder)
        {
            completeOrder.Order = Cart.CurrentInstance.Order;

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
                var orderUrl = Url.Action("Details", "Orders", new { id = completeOrder.Order.Id, area = RouteConstants.AdminAreaName }, Request.Url.Scheme);
                Task.Run(() =>
                    NotificationService.NotifySeller(completeOrder.Order.OrderNumber, orderUrl,
                        completeOrder.Order.SellerId));
                return RedirectToAction("OrderCompleted", new { number = orderNumber });
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
            return View(completeOrder);
        }

        public ActionResult GetCart()
        {
            var cart = Cart.CurrentInstance.Order;
            return PartialView("_CartPartial", cart);
        }

        public ActionResult Index()
        {
            var cart = Cart.CurrentInstance;
            using (var db = new ApplicationDbContext())
            {
                foreach (var orderProduct in cart.Order.OrderProducts)
                {
                    var product = db.Products.Include(entry => entry.Currency).FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                    orderProduct.ProductName = product.Name;
                    orderProduct.ProductPrice = (double)(product.Price * product.Currency.Rate);
                    foreach (var orderProductOption in orderProduct.OrderProductOptions)
                    {
                        var productOption = db.ProductOptions.Find(orderProductOption.ProductOptionId);
                        orderProductOption.ProductOptionName = productOption.Name;
                        orderProductOption.ProductOptionPriceGrowth = productOption.PriceGrowth;
                    }
                }
            }

            return View(cart.Order);
        }
    }
}