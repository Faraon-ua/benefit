﻿using Benefit.Common.Constants;
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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Benefit.Web.Controllers
{
    public class CartController : Controller
    {
        public OrderService OrderService = new OrderService();
        public UserService UserService = new UserService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        [FetchCategories(Order = 1)]
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

        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public ActionResult Order(string id)
        {
            var model = new CompleteOrderViewModel
            {
                SellerId = id,
                Order = Cart.CurrentInstance.Orders.FirstOrDefault(entry => entry.SellerId == id)
            };
            var domainSeller = ViewBag.Seller as Seller;
            ViewResult view = null;
            if (domainSeller != null)
            {
                var layoutName = string.Format("~/Views/SellerArea/{0}/Cart.cshtml",
                    domainSeller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                view = View(layoutName, model);
            }
            else
            {
                view = View(model);
            }
            if (model.Order == null)
            {
                return view;
            }

            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers
                    .Include(entry => entry.Promotions)
                    .Include(entry => entry.AssociatedSellers)
                    .FirstOrDefault(entry => entry.Id == id);
                var userId = User.Identity.GetUserId();
                if (seller == null)
                {
                    throw new HttpException(404, "Not found");
                }

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

                var sellersInOrder = model.Order.OrderProducts.Select(entry => entry.SellerId).Distinct().Count();
                model.ShippingMethods = db.ShippingMethods.Where(entry => entry.SellerId == id).ToList();
                if (sellersInOrder > 1 && seller.AssociatedSellers.Any())
                {
                    var associatedSellerIds = seller.AssociatedSellers.Select(ac => ac.Id).ToList();
                    var associatedShippingMethods = db.ShippingMethods.Where(entry => associatedSellerIds.Contains(entry.SellerId)).ToList();
                    model.ShippingMethods = model.ShippingMethods
                        .Intersect(associatedShippingMethods, new ShippingMethodComparer()).ToList();
                }
                if (userId != null)
                {
                    model.Addresses = db.Addresses.Include(entry => entry.Region).Where(entry => entry.UserId == userId)
                        .ToList();
                }
                if (seller.IsPrePaidPaymentActive)
                {
                    model.PaymentTypes.Add(PaymentType.PrePaid);
                }

                if (seller.IsPostPaidPaymentActive)
                {
                    model.PaymentTypes.Add(PaymentType.PostPaid);
                }

                if (seller.IsCashPaymentActive)
                {
                    model.PaymentTypes.Add(PaymentType.Cash);
                }

                if (seller.IsAcquiringActive)
                {
                    model.PaymentTypes.Add(PaymentType.Acquiring);
                }

                if (seller.IsBonusesPaymentActive)
                {
                    model.PaymentTypes.Add(PaymentType.Bonuses);
                }

                if (sellersInOrder > 1 && seller.AssociatedSellers.Any())
                {
                    if (!seller.AssociatedSellers.Any(entry => entry.IsPrePaidPaymentActive))
                    {
                        model.PaymentTypes.Remove(PaymentType.PrePaid);
                    }

                    if (!seller.AssociatedSellers.Any(entry => entry.IsPostPaidPaymentActive))
                    {
                        model.PaymentTypes.Remove(PaymentType.PostPaid);
                    }

                    if (!seller.AssociatedSellers.Any(entry => entry.IsCashPaymentActive))
                    {
                        model.PaymentTypes.Remove(PaymentType.Cash);
                    }

                    if (!seller.AssociatedSellers.Any(entry => entry.IsAcquiringActive))
                    {
                        model.PaymentTypes.Remove(PaymentType.Acquiring);
                    }

                    if (!seller.AssociatedSellers.Any(entry => entry.IsBonusesPaymentActive))
                    {
                        model.PaymentTypes.Remove(PaymentType.Bonuses);
                    }
                }
            }

            return view;
        }

        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public ActionResult OrderCompleted(string number)
        {
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                if (seller != null)
                {
                    var layoutName = string.Format("~/Views/SellerArea/{0}/OrderCompleted.cshtml",
                        seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                    return View(layoutName, model: number);
                }
            }
            return View(model: number);
        }

        [HttpPost]
        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public async Task<ActionResult> Order(CompleteOrder completeOrder)
        {
            var order = Cart.CurrentInstance.Orders.FirstOrDefault(entry => entry.SellerId == completeOrder.SellerId);
            completeOrder.Order = AutoMapper.Mapper.Map<Order>(order);
            ModelState.Remove("Order.UserId");
            var user = UserService.GetUser(User.Identity.GetUserId() ?? completeOrder.UserId);
            if(user == null)
            {
                ModelState.AddModelError("User", "Будь ласка увійдіть або зареєструйтесь");
            }
            if (completeOrder.Order == null)
            {
                throw new HttpException(404, "Not found");
            }

            if (completeOrder.PaymentType == PaymentType.Bonuses)
            {
                var sum = Cart.CurrentInstance.GetOrderSum();

                if (user.BonusAccount < sum)
                {
                    ModelState.AddModelError("PaymentType", "Недостатньо бонусів на рахунку");
                }
            }

            if (ViewBag.Seller == null && completeOrder.AddressId == null && string.IsNullOrEmpty(completeOrder.NewAddressLine) && string.IsNullOrEmpty(completeOrder.ShippingAddress))
            {
                ModelState.AddModelError("AddressId", "Оберіть адресу доставки");
            }
            var vm = new CompleteOrderViewModel();
            if (ModelState.IsValid)
            {
                if (completeOrder.AddressId == null)
                {
                    var newAddress = new Address
                    {
                        Id = Guid.NewGuid().ToString(),
                        AddressLine = completeOrder.NewAddressLine,
                        FullName = user.FullName,
                        Phone = user.PhoneNumber,
                        RegionId = user.RegionId,
                        UserId = user.Id
                    };
                    using (var db = new ApplicationDbContext())
                    {
                        db.Addresses.Add(newAddress);
                        db.SaveChanges();
                    }
                    completeOrder.AddressId = newAddress.Id;
                }

                completeOrder.Order.UserId = user.Id;
                completeOrder.Order.UserName = user.FullName;
                completeOrder.Order.UserPhone = user.PhoneNumber;
                completeOrder.Order.ShippingAddress = completeOrder.ShippingAddress.Clear();
                var orderNumber = OrderService.AddOrder(completeOrder);

                //order notifications
                var NotificationService = new NotificationsService();
                var orderUrl = Url.Action("details", "orders", new { id = completeOrder.Order.Id, area = RouteConstants.AdminAreaName }, Request.Url.Scheme);
                await NotificationService.NotifySeller(completeOrder.Order.OrderNumber, orderUrl, completeOrder.Order.SellerId).ConfigureAwait(false);
                return RedirectToAction("ordercompleted", new { number = orderNumber });
            }
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers.FirstOrDefault(entry => entry.Id == completeOrder.Order.SellerId);
                if (seller == null)
                {
                    throw new HttpException(404, "Not found");
                }

                var userId = user.Id;
                vm.ShippingMethods = db.ShippingMethods.Where(entry => entry.SellerId == completeOrder.Order.SellerId).ToList();
                vm.Addresses =
                    db.Addresses.Include(entry => entry.Region).Where(entry => entry.UserId == userId).ToList();
                if (seller.IsPrePaidPaymentActive)
                {
                    vm.PaymentTypes.Add(PaymentType.PrePaid);
                }

                if (seller.IsPostPaidPaymentActive)
                {
                    vm.PaymentTypes.Add(PaymentType.PostPaid);
                }

                if (seller.IsCashPaymentActive)
                {
                    vm.PaymentTypes.Add(PaymentType.Cash);
                }

                if (seller.IsAcquiringActive)
                {
                    vm.PaymentTypes.Add(PaymentType.Acquiring);
                }

                if (seller.IsBonusesPaymentActive)
                {
                    vm.PaymentTypes.Add(PaymentType.Bonuses);
                }
            }
            vm.Order = order;
            vm.SellerId = completeOrder.SellerId;

            TempData["ErrorMessage"] = ModelState.ModelStateErrors();
            var domainSeller = ViewBag.Seller as Seller;
            if (domainSeller != null)
            {
                var layoutName = string.Format("~/Views/SellerArea/{0}/Cart.cshtml",
                    domainSeller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return View(layoutName, vm);
            }
            return View(vm);
        }

        [FetchSeller]
        public ActionResult GetCart()
        {
            var cart = Cart.CurrentInstance.Orders;
            var domainSeller = ViewBag.Seller as Seller;
            if (domainSeller != null)
            {
                cart = cart.Where(entry => entry.SellerId == domainSeller.Id).ToList();
                var layoutName = string.Format("~/Views/SellerArea/{0}/_CartPartial.cshtml",
                    domainSeller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return PartialView(layoutName, cart);
            }
            return PartialView("_CartPartial", cart);
        }
    }
}