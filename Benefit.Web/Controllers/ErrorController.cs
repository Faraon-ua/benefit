using System.Web.Mvc;

namespace Benefit.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult NotFound()
        {
            return View();
        }
	}
}