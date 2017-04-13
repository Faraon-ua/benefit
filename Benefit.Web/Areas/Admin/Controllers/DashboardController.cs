using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.OrdersManagerRoleName + ", " + DomainConstants.AdminRoleName + ", " + DomainConstants.SellerRoleName + ", " + DomainConstants.SellerModeratorRoleName + ", " + DomainConstants.SellerOperatorRoleName)]
    public class DashboardController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        //
        // GET: /Admin/Dashboard/
        public ActionResult Index(string date)
        {
            var revenue = db.CompanyRevenues.OrderByDescending(entry => entry.Stamp).FirstOrDefault();
            if (date != null)
            {
                var format = "dd-MM-yyyy";
                var provider = CultureInfo.InvariantCulture;
                var dateDT = DateTime.ParseExact(date, format, provider).Date;
                revenue =
                    db.CompanyRevenues.FirstOrDefault(
                        entry =>
                            entry.Stamp.Year == dateDT.Year &&
                            entry.Stamp.Month == dateDT.Month &&
                            entry.Stamp.Day == dateDT.Day);
            }
            return View(revenue);
        }
    }
}