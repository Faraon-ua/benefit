﻿using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.RestApi.Filters;
using Benefit.Services;
using Benefit.Services.Admin;
using Benefit.Services.ExternalApi;
using Benefit.Services.Import;
using NLog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Benefit.RestApi.Controllers
{
    [CustomKeyAuth]
    public class ScheduleController : Controller
    {
        private ScheduleService ScheduleService = new ScheduleService();
        private Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public ActionResult ProcessImportTasks()
        {
            using (var db = new ApplicationDbContext())
            {
                var importTasks =
                db.ExportImports
                .Include(entry => entry.Seller)
                .Where(entry => entry.SyncType != SyncType.YmlExport && entry.SyncType != SyncType.YmlExportEpicentr && entry.SyncType != SyncType.YmlExportProm && entry.IsActive).ToList();
                _logger.Info("DB get success import tasks for " + string.Join(",", importTasks.Select(entry => entry.Seller.Name)));
                Task.Run(() =>
                {
                    foreach (var importTask in importTasks)
                    {
                        _logger.Info("Resolving import task for " + importTask.Seller.Name);
                        if (importTask.LastSync.HasValue && (DateTime.UtcNow - importTask.LastSync.Value).TotalHours < importTask.SyncPeriod * 23)
                        {
                            _logger.Info("Canceling import task for " + importTask.Seller.Name);
                            continue;
                        }
                        ImportServiceFactory.GetImportServiceInstance(importTask.SyncType).Import(importTask.Id);
                    }
                });
            }
            return Content("Ok");
        }

        public async Task<ActionResult> ProcessMarketplaceOrders()
        {
            var result = new StringBuilder();
            foreach (MarketplaceType type in Enum.GetValues(typeof(MarketplaceType)))
            {
                var key = string.Format("Is{0}Processing", type.ToString());
                if (HttpRuntime.Cache[key] == null)
                {
                    HttpRuntime.Cache[key] = false;
                }
                if ((bool)HttpRuntime.Cache[key] == false)
                {
                    HttpRuntime.Cache[key] = true;
                    var service = BaseMarketPlaceApi.GetMarketplaceServiceInstance(type);
                    try
                    {
                        await service.ProcessOrders();
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(string.Format("Export for {0} failed. ", type.ToString()) + ex.ToString());
                    }
                    finally
                    {
                        HttpRuntime.Cache[key] = false;
                    }
                    result.AppendFormat("Замовлення з {0} успішно синхронізовані<br/>", type.ToString());
                }
                else
                {
                    result.AppendFormat("Синхронізація замовленнь з {0} в процесі...<br/>", type.ToString());
                }
            }
            return Content(result.ToString());
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
                foreach (var product in db.Products.Where(entry => entry.IsRecommended))
                {
                    product.IsRecommended = false;
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
                foreach (var seller in db.Sellers.Where(entry => entry.IsActive && entry.GenerateRecommendedProducts))
                {
                    var recommendedProducts =
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
                    foreach (var rProduct in recommendedProducts)
                    {
                        rProduct.IsRecommended = true;
                    }
                }

                db.SaveChanges();
            }
            return Content("Ok");
        }
        public ActionResult ProcessHangingBonuses()
        {
            using (var db = new ApplicationDbContext())
            {
                var dueDate = DateTime.UtcNow.AddDays(-14);
                var transactions = db.Transactions.Where(entry => !entry.IsProcessed && entry.Time < dueDate && entry.Type == TransactionType.CashbackBonus).ToList();
                foreach (var transaction in transactions)
                {
                    var user = db.Users.Find(transaction.PayeeId);
                    //add transaction for personal purchase
                    var bonusTransferTransaction = new Transaction()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Bonuses = transaction.Bonuses,
                        BonusesBalans = user.BonusAccount + transaction.Bonuses,
                        OrderId = transaction.OrderId,
                        PayeeId = user.Id,
                        Time = DateTime.UtcNow,
                        Type = TransactionType.HangingToGeneral
                    };
                    db.Transactions.Add(bonusTransferTransaction);
                    user.BonusAccount = bonusTransferTransaction.BonusesBalans;
                    user.HangingBonusAccount -= transaction.Bonuses;
                    transaction.IsProcessed = true;
                    db.Entry(user).State = EntityState.Modified;
                    db.Entry(transaction).State = EntityState.Modified;

                    //10% from partner cashback
                    var mentor = db.Users.Find(user.ReferalId);
                    if (mentor != null)
                    {
                        var mentorTransaction = new Transaction()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Bonuses = transaction.Bonuses * 0.1,
                            BonusesBalans = mentor.BonusAccount + transaction.Bonuses * 10 / 100,
                            PayerId = user.Id,
                            PayeeId = mentor.Id,
                            Time = DateTime.UtcNow,
                            Type = TransactionType.CashbackMentorBonus
                        };
                        db.Transactions.Add(mentorTransaction);
                        mentor.BonusAccount = mentorTransaction.BonusesBalans;
                        db.Entry(mentor).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
            }
            return Content("Бонуси рохраховані");
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