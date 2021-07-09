using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Services;
using Benefit.Services.Admin;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Helpers;
using System.Web;
using Benefit.Domain.Models;
using Benefit.Web.Filters;
using System.Collections;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.SuperAdminRoleName)]
    public class AdminStuffController : AdminController
    {
        ScheduleService ScheduleService = new ScheduleService();

        //todo: change controller name
        //
        // GET: /Admin/AdminStuff/
        public ActionResult Index()
        {
            return View();
        }

        [FetchSeller]
        public ActionResult GenerateSitemap()
        {
            var siteMapHelper = new SiteMapHelper();
            siteMapHelper.Generate(Url, Request.Url.Scheme + "://" + Request.Url.Host, true, (ViewBag.Seller as Seller));
            TempData["SuccessMessage"] = "Файл sitemap.xml згенеровано";
            return RedirectToAction("Index");
        }

        public ActionResult ClosePeriod()
        {
            ScheduleService.CloseQualificationPeriod();
            TempData["SuccessMessage"] = "Період було закрито";
            return View("Index");
        }

        public ActionResult BonusesCalculation()
        {
            var result = ScheduleService.ProcessBonuses();
            if (result == null) return View("BonusesCalculationError");
            var bonusesHtml = ControllerContext.RenderPartialToString("_BonusesCalculationPartial", result);
            var emailService = new EmailService();
            emailService.SendBonusesRozrahunokResults(bonusesHtml);
            return View(result);
        }

        public ActionResult ClearCategoriesCache()
        {
            HttpRuntime.Cache.Remove("Categories");
            var staleItem = Url.Action("Index", "Home", new
            {
                area = string.Empty
            });
            Response.RemoveOutputCacheItem(staleItem);
            IDictionaryEnumerator enumerator = HttpRuntime.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string key = (string)enumerator.Key;
                if (key.Contains("Seller"))
                {
                    HttpRuntime.Cache.Remove(key);
                }
            }
            TempData["SuccessMessage"] = "Кеш категорій очищено";
            return RedirectToAction("Index");
        }
    }
}