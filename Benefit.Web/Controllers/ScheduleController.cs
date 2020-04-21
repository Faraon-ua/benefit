using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Services.Admin;
using Benefit.Services.Domain;
using Benefit.Services.ExternalApi;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;

namespace Benefit.Web.Controllers
{
    [CustomKeyAuth]
    public class ScheduleController : Controller
    {
        private ScheduleService ScheduleService = new ScheduleService();

        public ActionResult ProcessImportTasks()
        {
            Task.Run(() =>
            {
                using (var importService = new ImportExportService())
                {
                    importService.ProcessYmlImportTasks();
                }
            });
            return Content("Ok");
        }

        public ActionResult ProcessRozetkaOrders()
        {
            var rozetkaService = new RozetkaApiService();
            rozetkaService.ProcessOrders();
            return Content("Замовлення з Розетка успішно синхронізовані");
        }

        public ActionResult CheckSellersToBlock()
        {
            using (var db = new ApplicationDbContext())
            {
                var emailService = new EmailService();
                var sellersToBlock = db.Sellers.Include(entry => entry.Owner).Where(entry => entry.BlockOn != null).ToList();
                foreach (var seller in sellersToBlock)
                {
                    if (DateTime.UtcNow > seller.BlockOn)
                    {
                        seller.BlockOn = null;
                        seller.IsActive = false;
                    }
                    else
                    {
                        var days = (seller.BlockOn.Value - DateTime.UtcNow).Days;
                        emailService.SendSellerBlockAlert(seller.Owner.Email, days);
                    }
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

        public ActionResult FetchFeaturedProducts()
        {
            using (var db = new ApplicationDbContext())
            {
                foreach (var product in db.Products.Where(entry => entry.IsFeatured))
                {
                    product.IsFeatured = false;
                }
                foreach (var product in db.Products.Where(entry => entry.IsNewProduct))
                {
                    product.IsNewProduct = false;
                }

                foreach (var seller in db.Sellers.Where(entry => entry.GenerateFeaturedProducts))
                {
                    var featuredProducts =
                        db.Products
                            .Include(entry => entry.Images)
                            .Include(entry => entry.Category)
                            .Where(entry =>
                                entry.IsActive &&
                                (entry.AvailabilityState == ProductAvailabilityState.Available ||
                                 entry.AvailabilityState == ProductAvailabilityState.AlwaysAvailable) &&
                                entry.Images.Any() && entry.SellerId == seller.Id &&
                                (!entry.Category.IsSellerCategory ||
                                 (entry.Category.IsSellerCategory && entry.Category.MappedParentCategoryId != null)))
                            .OrderBy(entry => Guid.NewGuid())
                            .Take(ListConstants.FeaturedProductsPerSellerNumber);
                    foreach (var featuredProduct in featuredProducts)
                    {
                        featuredProduct.IsFeatured = true;
                    }
                    var newProducts =
                        db.Products
                            .Include(entry => entry.Images)
                            .Where(entry =>
                                entry.IsActive &&
                                (entry.AvailabilityState == ProductAvailabilityState.Available ||
                                 entry.AvailabilityState == ProductAvailabilityState.AlwaysAvailable) &&
                                entry.Images.Any() && entry.SellerId == seller.Id &&
                                (!entry.Category.IsSellerCategory ||
                                 (entry.Category.IsSellerCategory && entry.Category.MappedParentCategoryId != null)))
                            .OrderBy(entry => Guid.NewGuid())
                            .Take(ListConstants.FeaturedProductsPerSellerNumber);
                    foreach (var newProduct in newProducts)
                    {
                        newProduct.IsNewProduct = true;
                    }
                }

                db.SaveChanges();
            }
            return Content("Ok");
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
                if (existing != null)
                {
                    return Content("Company revenue was already saved");
                }

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

        public ActionResult ProcessCurrenciesTask()
        {
            ScheduleService.UpdateCurrencies();
            return Content("Currencies Ok");
        }
    }
}