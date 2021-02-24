using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.RestApi.Filters;
using Benefit.Services;
using Benefit.Services.Admin;
using Benefit.Services.ExternalApi;
using Benefit.Services.Import;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Benefit.RestApi.Controllers
{
    [CustomKeyAuth]
    public class ScheduleController : Controller
    {
        private ScheduleService ScheduleService = new ScheduleService();

        public ActionResult ProcessImportTasks()
        {
            using (var db = new ApplicationDbContext())
            {
                var importTasks =
                db.ExportImports
                .Include(entry => entry.Seller)
                .Include(entry => entry.Seller.MappedCategories)
                .Where(entry => entry.IsActive).ToList();
                Task.Run(() =>
                {
                    foreach (var importTask in importTasks)
                    {
                        if (importTask.LastSync.HasValue && (DateTime.UtcNow - importTask.LastSync.Value).TotalDays < importTask.SyncPeriod)
                        {
                            continue;
                        }
                        ImportServiceFactory.GetImportServiceInstance(importTask.SyncType).Import(importTask.Id);
                    }
                });
            }
            return Content("Ok");
        }

        public ActionResult ProcessRozetkaOrders()
        {
            var rozetkaService = new RozetkaApiService();
            if (HttpContext.Application["IsRozetkaProcessing"] == null)
            {
                HttpContext.Application["IsRozetkaProcessing"] = false;
            }
            if ((bool)HttpContext.Application["IsRozetkaProcessing"] == false)
            {
                HttpContext.Application["IsRozetkaProcessing"] = true;
                rozetkaService.ProcessOrders();
            }
            else
            {
                return Content("Синхронізація замовленнь з Розетка в процесі...");
            }
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

                foreach (var seller in db.Sellers.Where(entry => entry.IsActive && entry.GenerateFeaturedProducts))
                {
                    var featuredProducts =
                        db.Products
                            .Include(entry => entry.Images)
                            .Include(entry => entry.Category)
                            .Where(entry =>
                                entry.IsActive &&
                                entry.Category.IsActive &&
                                entry.ModerationStatus == ModerationStatus.Moderated &&
                                ((entry.AvailableAmount > 0 &&
                                entry.AvailabilityState == ProductAvailabilityState.Available) ||
                                 entry.AvailabilityState == ProductAvailabilityState.AlwaysAvailable) &&
                                entry.Images.Any() && entry.SellerId == seller.Id &&
                                (!entry.Category.IsSellerCategory ||
                                 (entry.Category.IsSellerCategory && entry.Category.MappedParentCategoryId != null)))
                            .OrderBy(entry => Guid.NewGuid())
                            .Take(ListConstants.FeaturedProductsPerSellerNumber).ToList();
                    foreach (var featuredProduct in featuredProducts)
                    {
                        featuredProduct.IsFeatured = true;
                    }
                    var newProducts =
                        db.Products
                            .Include(entry => entry.Images)
                            .Include(entry => entry.Category)
                            .Where(entry =>
                                entry.IsActive &&
                                entry.Category.IsActive &&
                                entry.ModerationStatus == ModerationStatus.Moderated &&
                               ((entry.AvailableAmount > 0 &&
                                entry.AvailabilityState == ProductAvailabilityState.Available) ||
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