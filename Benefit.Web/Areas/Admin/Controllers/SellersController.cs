using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Web.Helpers;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models.Enums;
using Benefit.Services;
using Benefit.Services.Domain;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Models;
using Benefit.Web.Models.Admin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WebGrease.Css.Extensions;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class SellersController : AdminController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserManager<ApplicationUser> UserManager { get; set; }
        SellerService SellerService = new SellerService();

        public SellersController()
        {
            var userStore = new UserStore<ApplicationUser>(db);
            UserManager = new UserManager<ApplicationUser>(userStore);
        }

        public ActionResult DownloadIntelHexFile(string id)
        {
            var seller = db.Sellers.Find(id);
            var fileContent = SellerService.GenerateIntelHexFile(seller.TerminalLicense);
            return File(fileContent, System.Net.Mime.MediaTypeNames.Application.Octet, seller.UrlName + ".hex");
        }

        public ActionResult GetSellerGallery(string id)
        {
            var seller = db.Sellers.Find(id);
            return Json(seller.Images.Where(entry => entry.ImageType == ImageType.SellerGallery).Select(entry => new { entry.ImageUrl, entry.SellerId }), JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<Schedule> SetSellerSchedules()
        {
            for (var i = 0; i < 7; i++)
            {
                yield return new Schedule()
                {
                    Id = Guid.NewGuid().ToString(),
                    Day = (DayOfWeek)i
                };
            }
        }
        private IEnumerable<SellerBusinessLevelIndex> SetBusinessLevelIndexeses()
        {
            for (var i = 0; i < Enum.GetValues(typeof(BusinessLevel)).Length; i++)
            {
                yield return new SellerBusinessLevelIndex()
                {
                    Index = 1
                };
            }
        }

        public ActionResult CreateOrUpdatePromotion(Promotion promo)
        {
            promo.Id = promo.Id ?? Guid.NewGuid().ToString();
            if (ModelState.IsValid)
            {
                if (db.Promotions.Any(entry => entry.Id == promo.Id))
                {
                    db.Entry(promo).State = EntityState.Modified;
                }
                else
                {
                    db.Promotions.Add(promo);
                }
                db.SaveChanges();
                var promos = db.Promotions.Where(entry => entry.SellerId == promo.SellerId);
                return PartialView("_Promotions", promos.ToList());
            }
            return Json(new { error = ModelState.ModelStateErrors() });
        }

        public ActionResult DeletePromotion(string id)
        {
            var promo = db.Promotions.FirstOrDefault(entry => entry.Id == id);

            if (promo == null) return new HttpStatusCodeResult(200);
            db.Promotions.Remove(promo);
            db.SaveChanges();
            return new HttpStatusCodeResult(200);
        }
        public ActionResult GetPromotionForm(string sellerId, string id)
        {
            var promo = (id == null) ? new Promotion() { SellerId = sellerId } : db.Promotions.Find(id);
            return PartialView("_PromotionForm", promo);
        }
        public ActionResult SellersSearch(SellerFilterValues filters)
        {
            IQueryable<Seller> sellers = db.Sellers.Include("Owner").AsQueryable();
            if (filters.CategoryId == "all")
            {
                return PartialView("_SellersSearch", sellers);
            }
            if (!string.IsNullOrEmpty(filters.Search))
            {
                filters.Search = filters.Search.ToLower();
                sellers = sellers.Where(entry => entry.Name.ToString().Contains(filters.Search) ||
                    entry.Personnels.Any(pers => pers.CardNumber.Contains(filters.Search)));
            }
            else
            {
                if (!string.IsNullOrEmpty(filters.DateRange))
                {
                    var dateRangeValues = filters.DateRange.Split('-');
                    var startDate = DateTime.Parse(dateRangeValues.First());
                    var endDate = DateTime.Parse(dateRangeValues.Last());
                    sellers = sellers.Where(entry => entry.RegisteredOn >= startDate && entry.RegisteredOn <= endDate);
                }

                if (!string.IsNullOrEmpty(filters.CategoryId))
                {
                    sellers = sellers.Where(entry => entry.SellerCategories.Select(sc => sc.CategoryId).Contains(filters.CategoryId));
                }
                if (filters.TotalDiscountPercent.HasValue)
                {
                    sellers = sellers.Where(entry => entry.TotalDiscount == filters.TotalDiscountPercent.Value);
                }
                if (filters.UserDiscountPercent.HasValue)
                {
                    sellers = sellers.Where(entry => entry.UserDiscount == filters.UserDiscountPercent.Value);
                }
                sellers = sellers.Where(entry => entry.IsBenefitCardActive == filters.BenefitCard);
                sellers = sellers.Where(entry => entry.HasEcommerce == filters.Ecommerce);
            }

            return PartialView("_SellersSearch", sellers);
        }

        // GET: /Admin/Sellers/
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var catsList = new List<Category>();
            var cats = db.Categories.Include(entry => entry.ChildCategories).Where(entry => entry.ParentCategoryId == null && !entry.IsSellerCategory).ToList();
            catsList.AddRange(cats);
            catsList.AddRange(cats.SelectMany(entry => entry.ChildCategories).Where(entry=>!entry.IsSellerCategory));
            catsList = catsList.ToList().SortByHierarchy().ToList();
            var options = new SellerFilterOptions
            {
                Categories = catsList.Select(entry => new HierarchySelectItem() { Text = entry.Name, Value = entry.Id, Level = entry.HierarchicalLevel }).ToList(),
                PointRatio = SettingsService.DiscountPercentToPointRatio.Values.Distinct().Select(entry => new SelectListItem() { Text = entry + ":1", Value = entry.ToString() }).ToList(),
                TotalDiscountPercent = SettingsService.DiscountPercentToPointRatio.Keys.Select(entry => new SelectListItem() { Text = entry + " %", Value = entry.ToString() }).ToList(),
                UserDiscountPercent = SettingsService.RewardsPlan.UserDiscounts.Select(entry => new SelectListItem() { Text = entry + " %", Value = entry.ToString() }).ToList()
            };
            options.Categories.Insert(0, new HierarchySelectItem() { Text = "Всі постачальники", Value = "all", Level = 1 });
            options.Categories.Insert(0, new HierarchySelectItem() { Text = "Не обрано", Value = string.Empty, Level = 1 });
            return View(options);
        }

        /*  // GET: /Admin/Sellers/Create
          public ActionResult UpdateSellerByOwenrId(string ownerId)
          {
              var owner = db.Users.Find(ownerId);
              if (owner == null) return HttpNotFound();
              if (!owner.OwnedSellers.Any()) return HttpNotFound();
              return RedirectToAction("CreateOrUpdate", new { id = Seller.CurrentAuthorizedSellerId ?? owner.OwnedSellers.First().Id });
          }
  */
        public ActionResult CreateOrUpdate(string id = null)
        {
            var existingSeller =
                db.Sellers.Include(entry => entry.BusinessLevelIndexes)
                    .Include(entry => entry.Reviews)
                    .Include(entry => entry.Personnels)
                    .Include(entry => entry.Schedules)
                    .Include(entry => entry.ShippingMethods.Select(sp => sp.Region))
                    .Include(entry => entry.SellerCategories.Select(sc => sc.Category))
                    .Include(entry => entry.Promotions)
                    .FirstOrDefault(entry => entry.Id == id);
            var seller = new SellerViewModel()
            {
                Seller =
                    existingSeller ??
                    new Seller()
                    {
                        Schedules = SetSellerSchedules().ToList(),
                        BusinessLevelIndexes = SetBusinessLevelIndexeses().ToList()
                    }
            };
            if (!seller.Seller.Schedules.Any())
            {
                seller.Seller.Schedules = SetSellerSchedules().ToList();
            }
            if (!seller.Seller.BusinessLevelIndexes.Any())
            {
                seller.Seller.BusinessLevelIndexes = SetBusinessLevelIndexeses().ToList();
            }
            if (seller.Seller.ShippingDescription != null)
            {
                seller.Seller.ShippingDescription = seller.Seller.ShippingDescription.Replace("<br/>", Environment.NewLine);
            }
            seller.Seller.Schedules = seller.Seller.Schedules.OrderBy(entry => entry.Day).ToList();
            if (existingSeller != null)
            {
                existingSeller.SellerCategories = existingSeller.SellerCategories.OrderBy(entry => entry.Category.Order).ToList();
                seller.OwnerExternalId = existingSeller.Owner.ExternalNumber;
                if (existingSeller.BenefitCardReferal != null)
                    seller.BenefitCardReferaExternalId = existingSeller.BenefitCardReferal.ExternalNumber;
                if (existingSeller.WebSiteReferal != null)
                    seller.WebSiteReferaExternalId = existingSeller.WebSiteReferal.ExternalNumber;
            }
            return View(seller);
        }

        // POST: /Admin/Sellers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreateOrUpdate(SellerViewModel sellervm, HttpPostedFileBase sellerLogo, HttpPostedFileBase sellerFavicon)
        {
            ApplicationUser owner = null;
            ApplicationUser websiteReferal = null;
            ApplicationUser benefitCardReferal = null;
            if (sellervm.OwnerExternalId != 0)
            {
                owner = db.Users.FirstOrDefault(entry => entry.ExternalNumber == sellervm.OwnerExternalId);
            }
            if (owner == null)
                ModelState.AddModelError("ReferalNumber", "Власника з таким ID не знайдено");

            if (sellervm.WebSiteReferaExternalId != null && sellervm.WebSiteReferaExternalId != 0)
            {
                websiteReferal = db.Users.FirstOrDefault(entry => entry.ExternalNumber == sellervm.WebSiteReferaExternalId);
                if (websiteReferal == null)
                    ModelState.AddModelError("ReferalNumber", "Реферала веб сайту з таким ID не знайдено");
            }
            if (sellervm.BenefitCardReferaExternalId != null && sellervm.BenefitCardReferaExternalId != 0)
            {
                benefitCardReferal = db.Users.FirstOrDefault(entry => entry.ExternalNumber == sellervm.BenefitCardReferaExternalId);
                if (benefitCardReferal == null)
                    ModelState.AddModelError("ReferalNumber", "Реферала Benefit Card з таким ID не знайдено");
            }
            if (sellervm.Seller.Currencies.Any() &&
                sellervm.Seller.Currencies.Any(
                    entry => entry.Name == null || entry.Provider == null || entry.Rate == null))
            {
                ModelState.AddModelError("Currencies", "Курси валют заповнено невірно");
            }

            if (ModelState.IsValid)
            {
                var seller = sellervm.Seller;
                if (seller.OwnerId != owner.Id)
                {
                    //remove old owner a seller role if it was his only seller
                    var oldOwner = db.Users.AsNoTracking().Include(entry => entry.OwnedSellers).FirstOrDefault(entry => entry.Id == seller.OwnerId);
                    if (oldOwner != null && oldOwner.OwnedSellers.Count == 1)
                    {
                        UserManager.RemoveFromRole(seller.OwnerId, DomainConstants.SellerRoleName);
                    }
                    //add new owner a seller role
                    UserManager.AddToRole(owner.Id, "Seller");
                    seller.OwnerId = owner.Id;
                }
                if (seller.ShippingDescription != null)
                {
                    seller.ShippingDescription = seller.ShippingDescription.Replace(Environment.NewLine, "<br/>");
                }
                seller.WebSiteReferalId = websiteReferal == null ? null : websiteReferal.Id;
                seller.BenefitCardReferalId = benefitCardReferal == null ? null : benefitCardReferal.Id;
                seller.LastModified = DateTime.UtcNow;
                seller.LastModifiedBy = User.Identity.Name;

                var sellerId = seller.Id ?? Guid.NewGuid().ToString();
                seller.Schedules.ForEach(entry => entry.SellerId = sellerId);

                if (seller.Id == null)
                {
                    seller.Id = sellerId;
                    seller.RegisteredOn = DateTime.UtcNow;
                    db.Sellers.Add(seller);
                    TempData["SuccessMessage"] = string.Format("Постачальника {0} було створено", seller.Name);
                }
                else
                {
                    seller.SellerCategories = seller.SellerCategories.Distinct(new SellerCategoryComparer()).ToList();
                    SellerService.ProcessCategories(seller.SellerCategories.ToList(), sellerId);

                    SellerService.ProcessAddresses(seller.Addresses.ToList(), sellerId);

                    SellerService.ProcessShippingMethods(seller.ShippingMethods.ToList(), sellerId);
                    db.Entry(seller).State = EntityState.Modified;
                    TempData["SuccessMessage"] = string.Format("Дані постачальника {0} було збережено", seller.Name);
                    db.SaveChanges();
                }

                //schedules
                if (!db.Schedules.Any(entry => entry.SellerId == sellerId))
                {
                    db.Schedules.AddRange(seller.Schedules);
                }
                else
                {
                    foreach (var schedule in seller.Schedules)
                    {
                        db.Entry(schedule).State = EntityState.Modified;
                    }
                }

                if (!db.SellerBusinessLevelIndexes.Any(entry => entry.SellerId == sellerId))
                {
                    db.SellerBusinessLevelIndexes.AddRange(seller.BusinessLevelIndexes);
                }
                else
                {
                    foreach (var businessLevel in seller.BusinessLevelIndexes)
                    {
                        db.Entry(businessLevel).State = EntityState.Modified;
                    }
                }

                var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                if (sellerLogo != null && sellerLogo.ContentLength != 0)
                {
                    var fileName = Path.GetFileName(sellerLogo.FileName);
                    var dotIndex = fileName.IndexOf('.');
                    var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
                    var pathString = Path.Combine(originalDirectory, "Images", ImageType.SellerLogo.ToString());
                    var img = new Image()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ImageType = ImageType.SellerLogo,
                        SellerId = seller.Id
                    };
                    img.ImageUrl = img.Id + fileExt;

                    db.Images.RemoveRange(
                        db.Images.Where(entry => entry.SellerId == seller.Id && entry.ImageType == ImageType.SellerLogo));
                    db.Images.Add(img);
                    sellerLogo.SaveAs(Path.Combine(pathString, img.ImageUrl));
                    var imagesService = new ImagesService();
                    imagesService.ResizeToSiteRatio(Path.Combine(pathString, img.ImageUrl), ImageType.SellerLogo);
                }
                if (sellerFavicon != null && sellerFavicon.ContentLength != 0)
                {
                    var fileName = Path.GetFileName(sellerFavicon.FileName);
                    var dotIndex = fileName.IndexOf('.');
                    var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
                    var pathString = Path.Combine(originalDirectory, "Images", ImageType.SellerFavicon.ToString());
                    var img = new Image()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ImageType = ImageType.SellerFavicon,
                        SellerId = seller.Id
                    };
                    img.ImageUrl = img.Id + fileExt;

                    db.Images.RemoveRange(
                        db.Images.Where(entry => entry.SellerId == seller.Id && entry.ImageType == ImageType.SellerFavicon));
                    db.Images.Add(img);
                    sellerFavicon.SaveAs(Path.Combine(pathString, img.ImageUrl));
                }
                db.SaveChanges();
                return RedirectToAction("CreateOrUpdate", new { id = seller.Id });
            }
            var scIds = sellervm.Seller.SellerCategories.Select(sc => sc.CategoryId).ToList();
            sellervm.Seller.SellerCategories =
                db.SellerCategories.Where(
                    entry => scIds.Contains(entry.CategoryId) && entry.SellerId == sellervm.Seller.Id).ToList();
            sellervm.Seller.Personnels = db.Personnels.Where(entry => entry.SellerId == sellervm.Seller.Id).ToList();
            sellervm.Seller.ShippingMethods.ForEach(entry => entry.Region = db.Regions.Find(entry.RegionId));
            return View(sellervm);
        }

        [HttpPost]
        public ActionResult LockUnlock(string id)
        {
            var existingSeller = db.Sellers.FirstOrDefault(entry => entry.Id == id);
            existingSeller.IsActive = !existingSeller.IsActive;
            db.Entry(existingSeller).State = EntityState.Modified;
            db.SaveChanges();
            return Json(existingSeller.IsActive);
        }
        [HttpPost]
        public ActionResult Delete(string id)
        {
            var categoriesService = new CategoriesService();
            var seller = db.Sellers.Find(id);
            var imagesService = new ImagesService();
            imagesService.DeleteAll(seller.Images, id, ImageType.SellerGallery, true, false);
            imagesService.DeleteAll(seller.Images, id, ImageType.SellerLogo, true, false);
            db.ExportImports.RemoveRange(db.ExportImports.Where(entry => entry.SellerId == id));
            db.Categories.Where(entry => entry.SellerId == id).ToList().ForEach(entry => categoriesService.Delete(entry.Id));
            db.Sellers.Remove(seller);
            db.SaveChanges();
            return Json(true);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult PromotionDetails(string id)
        {
            var promotionAccomplishers = db.PromotionAccomplishments.Include(entry=>entry.User).Where(entry => entry.PromotionId == id).ToList();
            return View(promotionAccomplishers);
        }
    }
}
