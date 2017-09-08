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
        public ApplicationDbContext db = new ApplicationDbContext();
        
        public ActionResult OnlineStatus(string name)
        {
            var sellers = db.Sellers.Where(entry => entry.IsBenefitCardActive && entry.IsActive);
            if (!string.IsNullOrEmpty(name))
            {
                sellers = sellers.Where(entry => entry.Name.ToLower().Contains(name.ToLower()));
            }
            return View(sellers.OrderByDescending(entry => entry.TerminalLastOnline).ToList());
        }
	}
}