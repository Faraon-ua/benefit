using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
            var user = UserService.GetUserInfo(User.Identity.GetUserId());
            return View(user);
        }

        public ActionResult Profile()
        {
            var user = UserService.GetUserInfo(User.Identity.GetUserId());
            return View(user);
        }
	}
}