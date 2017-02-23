using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.OrdersManagerRoleName + ", " + DomainConstants.AdminRoleName + ", " + DomainConstants.SellerRoleName + ", " + DomainConstants.SellerModeratorRoleName + ", " + DomainConstants.SellerOperatorRoleName)]
    public class DashboardController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        //
        // GET: /Admin/Dashboard/
        public ActionResult Index()
        {
            var model = new DashboardViewModel
            {
                BonusAcountsSum = db.Users.Sum(entry => entry.BonusAccount),
                TotalBonusAcountsSum = db.Users.Sum(entry => entry.TotalBonusAccount),
                HangingBonusAcountsSum = db.Users.Sum(entry => entry.HangingBonusAccount)
            };
            return View(model);
        }
    }
}