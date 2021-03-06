﻿using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Common.Helpers;
using Benefit.Services.ExternalApi;
using Benefit.Services;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        // GET: /Admin/Dashboard/
        [Authorize(Roles = DomainConstants.AdminRoleName)]
        public ActionResult Index(string date)
        {
            using (var db = new ApplicationDbContext())
            {
                var revenue = db.CompanyRevenues.OrderByDescending(entry => entry.Stamp).FirstOrDefault();
                if (date != null)
                {
                    var format = "dd-MM-yyyy";
                    var provider = CultureInfo.InvariantCulture;
                    var dateDT = DateTime.ParseExact(date, format, provider).Date;
                    revenue =
                        db.CompanyRevenues.FirstOrDefault(
                            entry =>
                                entry.Stamp.Year == dateDT.Year &&
                                entry.Stamp.Month == dateDT.Month &&
                                entry.Stamp.Day == dateDT.Day);
                }
                return View(revenue);
            }
        }

        [Authorize(Roles = DomainConstants.OrdersManagerRoleName + ", " + DomainConstants.AdminRoleName + ", " + DomainConstants.SellerRoleName + ", " + DomainConstants.SellerModeratorRoleName + ", " + DomainConstants.SellerOperatorRoleName)]
        public ActionResult Cabinet()
        {
            using (var db = new ApplicationDbContext())
            {
                var model = new SellerDashboard()
                {
                    Seller = db.Sellers.Include(entry => entry.Transactions).AsNoTracking().FirstOrDefault(entry => entry.Id == Seller.CurrentAuthorizedSellerId),

                };
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult CreateBill(double sum)
        {
            using (var db = new ApplicationDbContext())
            {
                var maxNumber = db.PaymentBills.Select(entry => entry.InnerNumber).DefaultIfEmpty(10000).Max() + 1;
                var bill = new PaymentBill()
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = BillType.Royalty,
                    Status = BillStatus.AwaitingPayment,
                    SellerId = Seller.CurrentAuthorizedSellerId,
                    Sum = sum,
                    Time = DateTime.UtcNow,
                    InnerNumber = maxNumber,
                    Number = string.Format("{0}-{1}", Enumerations.GetEnumDescription(BillType.Royalty), maxNumber.ToString("D8"))
                };
                db.PaymentBills.Add(bill);
                db.SaveChanges();
                return new HttpStatusCodeResult(200);
            }
        }

        [Authorize(Roles = DomainConstants.AdminRoleName)]
        public ActionResult GetNotifications()
        {
            using (var db = new ApplicationDbContext())
            {
                var reviewsToModerate = db.Reviews.Count(entry => !entry.IsActive);
                var sellerNewContent = db.ExportImports.Include(entry => entry.Seller).Where(entry => entry.HasNewContent).Select(entry => entry.Seller).ToList();
                var result = new NotificationsViewModel()
                {
                    Reviews = reviewsToModerate,
                    Total = reviewsToModerate + sellerNewContent.Count,
                    NewSellerContent = sellerNewContent
                };
                if (result.Total > 0)
                {
                    return PartialView("_Notifications", result);
                }
                return Content(string.Empty);
            }
        }
    }
}