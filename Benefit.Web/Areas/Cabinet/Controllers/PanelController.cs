﻿using System.Collections.Generic;
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

        public ActionResult Profile()
        {
            var user = UserService.GetUserInfoWithRegions(User.Identity.GetUserId());
            return View(user);
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