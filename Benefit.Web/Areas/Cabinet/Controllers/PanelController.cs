using System.Collections.Generic;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
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

        public ActionResult GetPartners(string id, int level)
        {
            var partners = UserService.GetPartners(id);
            return PartialView("_PartnersPartial", new KeyValuePair<int, IEnumerable<ApplicationUser>>(level, partners));
        }
	}
}