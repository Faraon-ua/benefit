using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Services.Admin;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Helpers;

namespace Benefit.Web.Areas.Admin.Controllers
{
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