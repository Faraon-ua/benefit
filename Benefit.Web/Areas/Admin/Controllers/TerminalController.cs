using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Web.Areas.Admin.Controllers.Base;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.AdminRoleName)]
    public class TerminalController : AdminController
    {
        [HttpGet]
        public ActionResult OnlineStatus(string search)
        {
            using (var db = new ApplicationDbContext())
            {
                var sellers = db.Sellers.Where(entry => entry.IsBenefitCardActive);
                if (!string.IsNullOrEmpty(search))
                {
                    sellers = sellers.Where(entry => entry.Name.ToLower().Contains(search.ToLower()));
                }
                return View(sellers.OrderByDescending(entry => entry.TerminalLastOnline).ToList());
            }
        }
	}
}