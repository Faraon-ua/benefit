using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Benefit.Web.Controllers
{
    [CustomKeyAuth]
    public class ScheduleController : Controller
    {
        public ActionResult GenerateSellerReport(int? year = null, int? month = null, string sellerId = null)
        {
            var sellerReportService = new SellerReportService();
            year = year ?? DateTime.Now.Year;
            month = month ?? DateTime.Now.Month - 1;
            var startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0);
            var endDate = new DateTime(year.Value, month.Value, DateTime.DaysInMonth(year.Value, month.Value), 23, 59, 59);
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var reportsPath = Path.Combine(originalDirectory, "Reports");
            using (var db = new ApplicationDbContext())
            {
                var sellerIds = db.Orders
                    .Where(entry => entry.Time > startDate && entry.Time < endDate)
                    .Select(entry => entry.SellerId).Distinct();
                if (sellerId != null)
                {
                    sellerIds = sellerIds.Where(entry => entry == sellerId);
                }
                foreach (var seller_id in sellerIds)
                {
                    var bytes = sellerReportService.GenerateSellerReport(seller_id, startDate, endDate);
                    var reportPath = Path.Combine(reportsPath, seller_id);
                    if (!Directory.Exists(reportPath))
                    {
                        Directory.CreateDirectory(reportPath);
                    }
                    System.IO.File.WriteAllBytes(
                        Path.Combine(reportPath, string.Format("{0}-{1}-{2}.xls", year.Value, month.Value, DateTime.Now.ToString("yyyyMMddhhmm"))), bytes);
                    var sellerReport = new SellerReport()
                    {
                        Date = DateTime.Now,
                        FileUrl = string.Format("{0}-{1}-{2}.xls", year.Value, month.Value, DateTime.Now.ToString("yyyyMMddhhmm")),
                        Id = Guid.NewGuid().ToString(),
                        SellerId = seller_id,
                        Month = month.Value,
                        Year = year.Value
                    };
                    db.SellerReports.Add(sellerReport);
                }
                db.SaveChanges();
            }
            return Content("Ok");
        }
        public ActionResult GenerateExportFiles(string exportId = null)
        {
            var exportService = new ImportExportService();
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            using (var db = new ApplicationDbContext())
            {
                var exports = db.ExportImports.Where(entry => entry.SyncType == SyncType.YmlExport).ToList();
                if (exportId != null)
                {
                    exports = exports.Where(entry => entry.Id == exportId).ToList();
                }
                foreach (var export in exports)
                {
                    var destPath = Path.Combine(originalDirectory, "Export", export.Name);
                    var isExists = Directory.Exists(destPath);
                    if (!isExists)
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    destPath = Path.Combine(destPath, "index.xml");
                    if (exportService.Export(export.Id, destPath) == null)
                    {
                        return Content("Export file was not generated");
                    }
                }
            }
            return Content("Export files generated");
        }

        public ActionResult GenerateSiteMap()
        {
            var siteMapHelper = new SiteMapHelper();
            var count = siteMapHelper.Generate(Url, Request.Url.Scheme + "://" + Request.Url.Host);
            return Content(count.ToString());
        }
    }
}