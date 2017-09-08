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
        
        public ActionResult OnlineStatus()
        {
            var sellers = db.Sellers.Where(entry => entry.IsBenefitCardActive).OrderByDescending(entry=>entry.TerminalLastOnline).ToList();
            return View(sellers);
        }
	}
}