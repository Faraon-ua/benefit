using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Data.Entity;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Cart;
using Benefit.Services.Domain;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Telerik.Web.Data.Extensions;

namespace Benefit.Web.Controllers
{
    public class CartController : Controller
    {
        public OrderService OrderService = new OrderService();
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
            return Json(new { redirectUrl = Url.Action("Order") });
        }

        public ActionResult Order()
        {
            var model = new CompleteOrderViewModel();
            model.Order = Cart.CurrentInstance.Order;
            var sellerId = Cart.CurrentInstance.Order.SellerId;
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers.FirstOrDefault(entry => entry.Id == sellerId);
                var userId = User.Identity.GetUserId();
                if (seller == null) return HttpNotFound();
                model.ShippingMethods = db.ShippingMethods.Where(entry => entry.SellerId == sellerId).ToList();
                model.Addresses = db.Addresses.Include(entry => entry.Region).Where(entry => entry.UserId == userId).ToList();
                model.PaymentTypes.Add(PaymentType.Cash);
                if (seller.IsAcquiringActive)
                    model.PaymentTypes.Add(PaymentType.Acquiring);
                if (seller.IsBonusesPaymentActive)
                    model.PaymentTypes.Add(PaymentType.Bonuses);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Order(CompleteOrderViewModel completeOrder)
        {
            completeOrder.Order = Cart.CurrentInstance.Order;
            if (ModelState.IsValid)
            {
                completeOrder.Order.UserId = User.Identity.GetUserId();
                OrderService.AddOrder(completeOrder);
                return View("OrderCompleted");
            }
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers.FirstOrDefault(entry => entry.Id == completeOrder.Order.SellerId);
                if (seller == null) return HttpNotFound();
                var userId = User.Identity.GetUserId();
                completeOrder.ShippingMethods = db.ShippingMethods.Where(entry => entry.SellerId == completeOrder.Order.SellerId).ToList();
                completeOrder.Addresses =
                    db.Addresses.Include(entry => entry.Region).Where(entry => entry.UserId == userId).ToList();
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