using System;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;

namespace Benefit.Web.Controllers
{
    [CustomKeyAuth]
    public class ScheduleController : Controller
    {
        public ActionResult ProcessImportTasks()
        {
            var importService = new ImportExportService();
            importService.ImportFromPromua();
            return Content("Ok");
        }

        public ActionResult GenerateSiteMap()
        {
            var siteMapHelper = new SiteMapHelper();
            var count = siteMapHelper.Generate(Url);
            return Content(count.ToString());
        }

        public ActionResult SaveCompanyRevenue()
        {
            using (var db = new ApplicationDbContext())
            {
                var existing =
                    db.CompanyRevenues.FirstOrDefault(
                        entry =>
                            entry.Stamp.Year == DateTime.UtcNow.Year && entry.Stamp.Month == DateTime.UtcNow.Month &&
                            entry.Stamp.Day == DateTime.UtcNow.Day);
                if (existing != null) return Content("Company revenue was already saved");
                var revenue = new CompanyRevenue
                {
                    Id = Guid.NewGuid().ToString(),
                    Stamp = DateTime.UtcNow,
                    TotalBonuses = db.Users.Sum(entry => entry.BonusAccount),
                    TotalEarnedBonuses = db.Users.Sum(entry => entry.TotalBonusAccount),
                    TotalHangingBonuses = db.Users.Sum(entry => entry.HangingBonusAccount)
                };
                db.CompanyRevenues.Add(revenue);
                db.SaveChanges();
                return Content("Company revenue saved");
            }
        }
    }
}