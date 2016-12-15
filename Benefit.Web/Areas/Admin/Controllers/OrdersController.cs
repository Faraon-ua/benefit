﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services.Domain;
using Benefit.Web.Helpers;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private OrderService OrderService = new OrderService();
        private TransactionsService TransactionsService = new TransactionsService();

        // GET: /Admin/Orders/
        public ActionResult Index()
        {
            //todo: add paging, remove take
            return View();
        }

        public ActionResult Print(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Include(entry=>entry.User).Include(entry => entry.OrderProducts).Include(entry => entry.OrderProductOptions).FirstOrDefault(entry => entry.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [HttpPost]
        public void UpdateStatus(OrderStatus orderStatus, string statusComment, string orderId)
        {
            var order = db.Orders.Find(orderId);
            order.Status = orderStatus;
            order.StatusComment = statusComment;

            //add points and bonuses if order finished
            if (orderStatus == OrderStatus.Finished)
            {
                TransactionsService.AddOrderTransaction(order);
            }
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
        }

        public ActionResult GetOrdersList(OrderType orderType, int page = 0)
        {
            var takePerPage = 50;
            var ordersTotal =
                db.Orders.Include(o => o.User)
                    .Where(entry => entry.OrderType == orderType)
                    .OrderByDescending(entry => entry.Time)
                    .Count();
            var orders = db.Orders.Include(o => o.User).Where(entry => entry.OrderType == orderType).OrderByDescending(entry => entry.Time).Skip(page * takePerPage).Take(takePerPage);
            var ordersHtml = ControllerContext.RenderPartialToString("_OrdersListPartial", new PaginatedList<Order>
            {
                Items = orders.ToList(),
                Pages = ordersTotal/takePerPage + 1,
                ActivePage = page
            });
            return Json(new
            {
                html = ordersHtml,
                hasNewOrder = db.Orders.Any(entry => entry.Status == OrderStatus.Created)
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteOrderProduct(string orderId, string productId)
        {
            var product =
                db.OrderProducts.FirstOrDefault(entry => entry.OrderId == orderId && entry.ProductId == productId);
            db.OrderProductOptions.RemoveRange(product.DbOrderProductOptions);
            db.OrderProducts.Remove(product);
            db.SaveChanges();
            var order = db.Orders.Include(entry => entry.OrderProducts).Include(entry => entry.OrderProductOptions).FirstOrDefault(entry => entry.Id == orderId);
            var orderSum =
             order.OrderProducts.Sum(
                 entry =>
                     entry.ProductPrice * entry.Amount +
                     entry.OrderProductOptions.Sum(option => option.ProductOptionPriceGrowth * option.Amount));
            order.Sum = orderSum;
            db.SaveChangesAsync();
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
            orderProduct.ProductId = Guid.NewGuid().ToString();
            db.OrderProducts.Add(orderProduct);
            var order = db.Orders.Include(entry => entry.OrderProducts).Include(entry => entry.OrderProductOptions).FirstOrDefault(entry => entry.Id == orderProduct.OrderId);
            var orderSum =
             order.OrderProducts.Sum(
                 entry =>
                     entry.ProductPrice * entry.Amount +
                     entry.OrderProductOptions.Sum(option => option.ProductOptionPriceGrowth * option.Amount));
            order.Sum = orderSum;

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
            var order = db.Orders.Include(entry => entry.OrderProducts).Include(entry => entry.OrderProductOptions).FirstOrDefault(entry => entry.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: /Admin/Orders/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Users, "Id", "FullName");
            return View();
        }

        // POST: /Admin/Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Sum,Description,PersonalBonusesSum,PointsSum,CardNumber,ShippingName,ShippingAddress,ShippingCost,Time,OrderType,PaymentType,Status,SellerId,SellerName,UserId")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "Id", "FullName", order.UserId);
            return View(order);
        }

        // GET: /Admin/Orders/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "FullName", order.UserId);
            return View(order);
        }

        // POST: /Admin/Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Sum,Description,PersonalBonusesSum,PointsSum,CardNumber,ShippingName,ShippingAddress,ShippingCost,Time,OrderType,PaymentType,Status,SellerId,SellerName,UserId")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "FullName", order.UserId);
            return View(order);
        }

        // GET: /Admin/Orders/Delete/5
        public ActionResult Delete(string id)
        {
            OrderService.DeleteOrder(id);
            return RedirectToAction("Index");
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
