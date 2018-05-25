using System.Web.Mvc;
using Benefit.Web.Filters;

namespace Benefit.Web.Controllers
{
    public class ErrorController : Controller
    {
        [FetchCategories]
        public ActionResult NotFound()
        {
            return View();
        }
	}
}