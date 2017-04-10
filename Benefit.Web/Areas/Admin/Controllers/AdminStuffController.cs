using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Services.Admin;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Helpers;
using System.Data.Entity;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.SuperAdminRoleName)]
    public class AdminStuffController : AdminController
    {
        ApplicationDbContext db = new ApplicationDbContext();
        //todo: change controller name
        //
        // GET: /Admin/AdminStuff/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GenerateSitemap()
        {
            var siteMapHelper = new SiteMapHelper();
            var count = siteMapHelper.Generate(Url);
            TempData["SuccessMessage"] = "Файл sitemap.xml згенеровано. Кількість лінків: " + count;
            return RedirectToAction("Index");
        }

        public ActionResult ClosePeriod()
        {
            var service = new ScheduleService();
            service.CloseQualificationPeriod();
            TempData["SuccessMessage"] = "Період було закрито";
            return View("Index");
        }

        public ActionResult BonusesCalculation()
        {
            var service = new ScheduleService();
            var result = service.ProcessBonuses();
            if (result == null) return View("BonusesCalculationError");
            var bonusesHtml = ControllerContext.RenderPartialToString("_BonusesCalculationPartial", result);
            var emailService = new EmailService();
            emailService.SendBonusesRozrahunokResults(bonusesHtml);
            return View(result);
        }

        public ActionResult ClearChat()
        {
            db.Messages.RemoveRange(db.Messages);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Чат було очищено";
            return View("Index");
        }
    }
}