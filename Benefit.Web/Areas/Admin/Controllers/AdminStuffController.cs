using System.Web.Mvc;
using Benefit.Services.Admin;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class AdminStuffController : Controller
    {

        //todo: change controller name
        //
        // GET: /Admin/AdminStuff/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult BonusesCalculation()
        {
            var service = new ScheduleService();
            var result = service.ProcessBonuses();
            if (result == null) return View("BonusesCalculationError");
            return View(result);
        }
    }
}