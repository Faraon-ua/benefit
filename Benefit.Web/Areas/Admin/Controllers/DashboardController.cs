using System.Web.Mvc;
using Benefit.Web.Areas.Admin.Controllers.Base;

namespace Benefit.Web.Areas.Admin.Controllers
{
//    [CustomAuthorization(Url = "/Area/Login")]
    [Authorize(Roles = "Admin,ContentManager")]
    public class DashboardController : AdminController
    {
        //
        // GET: /Admin/Dashboard/
        public ActionResult Index()
        {
            return View();
        }
	}
}