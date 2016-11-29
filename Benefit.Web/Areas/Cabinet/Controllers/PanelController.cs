using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using Benefit.Web.Helpers;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Areas.Cabinet.Controllers
{
    public class PanelController : BasePartnerController
    {
        private UserService UserService { get; set; }

        public PanelController()
        {
            UserService = new UserService();
        }

        [HttpPost]
        public ActionResult GetPartnersByReferalIds(string[] ids, int level)
        {
            if (ids == null)
            {
                return Json(new { });
            }
            var partners = UserService.GetPartnersByReferalIds(ids);
            var result = partners.ToDictionary(entry => entry.Key,
                entry => ControllerContext.RenderPartialToString("_PartnersPartial",
                    new KeyValuePair<int, IEnumerable<ApplicationUser>>(level, entry.Value)));
            var jsonResult = Json(result);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        //
        // GET: /Cabinet/Panel/
        public ActionResult Index()
        {
            var user = UserService.GetUserInfoWithRegions(User.Identity.GetUserId());
            return View(user);
        }

        public ActionResult GetOrderDetails(string orderId)
        {
            using (var db = new ApplicationDbContext())
            {
                var order =
                    db.Orders.Include(entry => entry.OrderProducts)
                        .Include(entry => entry.OrderProductOptions)
                        .FirstOrDefault(entry => entry.Id == orderId);
                return PartialView("_OrderDetailsPartial", order);
            }
        }

        public ActionResult Profile()
        {
            var user = UserService.GetUserInfoWithRegions(User.Identity.GetUserId());
            return View(user);
        }

        public ActionResult UserAddress(string id)
        {
            var userId = User.Identity.GetUserId();
            var user = UserService.GetUserInfoWithRegions(userId);
            ViewBag.User = user;
            Address address;
            using (var db = new ApplicationDbContext())
            {
                address = db.Addresses.FirstOrDefault(entry => entry.Id == id && entry.UserId == userId) ?? new Address();
                if (address.Region != null)
                {
                    var expandedRegionName = address.Region.ExpandedName;
                }
            }
            return View(address);
        }

        [HttpPost]
        public ActionResult UserAddress(Address address)
        {
            var userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                using (var db = new ApplicationDbContext())
                {
                    address.UserId = userId;
                    if (address.Id == null)
                    {
                        address.Id = Guid.NewGuid().ToString();
                        db.Addresses.Add(address);
                    }
                    else
                    {
                        db.Entry(address).State = EntityState.Modified; ;
                    }
                    db.SaveChanges();
                }
                return RedirectToAction("Profile");
            }
            var user = UserService.GetUserInfoWithRegions(userId);
            ViewBag.User = user;

            return View(address);
        }

        public ActionResult Structure()
        {
            var user = UserService.GetUserInfoWithPartners(User.Identity.GetUserId());
            return View(user);
        }

        public ActionResult planvunagorod()
        {
            var user = UserService.GetUserInfoWithRegions(User.Identity.GetUserId());
            using (var db = new ApplicationDbContext())
            {
                ViewBag.Page = db.InfoPages.FirstOrDefault(entry => entry.UrlName == "plan_vinagorod").Content;
                return View(user);
            }
        }

        public ActionResult Zakladu()
        {
            return View();
        }
    }
}