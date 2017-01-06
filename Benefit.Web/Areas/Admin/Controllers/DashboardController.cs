using System.Web.Mvc;
using Benefit.Common.Constants;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.OrdersManagerRoleName + ", " + DomainConstants.AdminRoleName + ", " + DomainConstants.SellerRoleName)]
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