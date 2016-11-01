using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
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

        [HttpPost]
        public ActionResult GetPartnersByReferalIds(string[] ids, int level)
        {
            var partners = UserService.GetPartnersByReferalIds(ids);
            var result = partners.ToDictionary(entry => entry.Key,
                entry => ControllerContext.RenderPartialToString("_PartnersPartial",
                    new KeyValuePair<int, IEnumerable<ApplicationUser>>(level, entry.Value)));
            return Json(result);
        }
    }
}