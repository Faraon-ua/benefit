using System.Web.Mvc;

namespace Benefit.Web.Areas.Admin.Controllers
{
//    [CustomAuthorization(Url = "/Area/Login")]
    [Authorize(Roles = "Admin,ContentManager")]
    public class DashboardController : Controller
    {
        //
        // GET: /Admin/Dashboard/
        public ActionResult Index()
        {
            return View();
        }
	}
}