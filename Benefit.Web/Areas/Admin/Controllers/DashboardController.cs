using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.AdminRoleName)]
    public class DashboardController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        //
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

        public ActionResult GetNotifications()
        {
            var reviewsToModerate = db.Reviews.Count(entry => !entry.IsActive);
            var sellerNewContent = db.ExportImports.Include(entry=>entry.Seller).Where(entry => entry.HasNewContent).Select(entry=>entry.Seller).ToList();
            var result = new NotificationsViewModel()
            {
                Reviews = reviewsToModerate,
                Total = reviewsToModerate + sellerNewContent.Count,
                NewSellerContent = sellerNewContent
            };
            if (result.Total > 0)
            {
                return PartialView("_Notifications", result);
            }
            return Content(string.Empty);
        }
    }
}