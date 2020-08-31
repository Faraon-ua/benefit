using Benefit.Services.ExternalApi;
using System.Web.Mvc;

namespace Benefit.RestApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult ProcessRozetkaOrders()
        {
            var rozetkaService = new RozetkaApiService();
            rozetkaService.ProcessOrders();
            return Content("Замовлення з Розетка успішно синхронізовані");
        }
    }
}
