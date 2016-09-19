using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Models.Admin;
using WebGrease.Css.Extensions;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class SellersController : AdminController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult GetSellerGallery(string id)
        {
            var seller = db.Sellers.Find(id);
            return Json(seller.Images.Where(entry => entry.ImageType == ImageType.SellerGallery).Select(entry => new { entry.ImageUrl }), JsonRequestBehavior.AllowGet);
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
        public ActionResult SellersSearch(SellerFilterValues filters)
        {
            IQueryable<Seller> sellers = db.Sellers.Include("Owner").AsQueryable();
            if (!string.IsNullOrEmpty(filters.Search))
            {
                filters.Search = filters.Search.ToLower();
                sellers = sellers.Where(entry => entry.Owner.FullName.ToLower().Contains(filters.Search) ||
                                                 entry.Name.ToString().Contains(filters.Search));
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
                    sellers = sellers.Where(entry => entry.SellerCategories.Select(sc=>sc.CategoryId).Contains(filters.CategoryId));
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
        public ActionResult Index()
        {
            var routeCats = db.Categories.Where(entry => entry.ParentCategoryId == null).OrderBy(entry => entry.Name);
            var options = new SellerFilterOptions
            {
                Categories = routeCats.Select(entry => new SelectListItem() { Text = entry.Name, Value = entry.Id }),
                PointRatio = SettingsService.DiscountPercentToPointRatio.Values.Distinct().Select(entry => new SelectListItem() { Text = entry + ":1", Value = entry.ToString() }),
                TotalDiscountPercent = SettingsService.DiscountPercentToPointRatio.Keys.Select(entry => new SelectListItem() { Text = entry + " %", Value = entry.ToString() }),
                UserDiscountPercent = SettingsService.UserDiscounts.Select(entry => new SelectListItem() { Text = entry + " %", Value = entry.ToString() })
            };
            return View(options);
        }

        // GET: /Admin/Sellers/Create
        public ActionResult CreateOrUpdate(string id = null)
        {
            var existingSeller = db.Sellers.Include("Schedules").Include(entry => entry.ShippingMethods.Select(sp => sp.Region)).FirstOrDefault(entry => entry.Id == id);
            var seller = new SellerViewModel()
            {
                Seller = existingSeller ?? new Seller() { Schedules = SetSellerSchedules().ToList() }
            };
            if (!seller.Seller.Schedules.Any())
            {
                seller.Seller.Schedules = SetSellerSchedules().ToList();
            }
            seller.Seller.Schedules = seller.Seller.Schedules.OrderBy(entry => entry.Day).ToList();
            if (existingSeller != null)
            {
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
        public ActionResult CreateOrUpdate(SellerViewModel sellervm)
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
                seller.OwnerId = owner.Id;
                seller.WebSiteReferalId = websiteReferal == null ? null : websiteReferal.Id;
                seller.BenefitCardReferalId = benefitCardReferal == null ? null : benefitCardReferal.Id;
                seller.LastModified = DateTime.UtcNow;
                seller.LastModifiedBy = User.Identity.Name;

                var sellerId = seller.Id ?? Guid.NewGuid().ToString();
                seller.Currencies.ForEach(entry => entry.SellerId = sellerId);
                seller.Addresses.ForEach(entry => entry.SellerId = sellerId);
                seller.ShippingMethods.ForEach(entry => entry.SellerId = sellerId);
                seller.Schedules.ForEach(entry => entry.SellerId = sellerId);

                var currenciesToAdd = seller.Currencies.Where(entry => entry.Id == null).ToList();
                currenciesToAdd.ForEach(entry => entry.Id = Guid.NewGuid().ToString());

                var addressesToAdd = seller.Addresses.Where(entry => entry.Id == null).ToList();
                addressesToAdd.ForEach(entry => entry.Id = Guid.NewGuid().ToString()); 
                
                var shippingMethodsToAdd = seller.ShippingMethods.Where(entry => entry.Id == null).ToList();
                shippingMethodsToAdd.ForEach(entry => entry.Id = Guid.NewGuid().ToString());

                if (seller.Id == null)
                {
                    seller.Id = sellerId;
                    seller.RegisteredOn = DateTime.UtcNow;
                    db.Sellers.Add(seller);
                    TempData["SuccessMessage"] = string.Format("Постачальника {0} було створено", seller.Name);
                }
                else
                {
                    db.Entry(seller).State = EntityState.Modified;
                    TempData["SuccessMessage"] = string.Format("Дані постачальника {0} було збережено", seller.Name);
                    //to remove
                    var existingSeller = db.Sellers.Find(sellerId);
                    var addressesToRemove =
                        existingSeller.Addresses.Where(
                            entry => !seller.Addresses.Select(addr => addr.Id).Contains(entry.Id)).ToList();
                    db.Addresses.RemoveRange(addressesToRemove);
                }
                db.Currencies.AddRange(currenciesToAdd);
                db.Addresses.AddRange(addressesToAdd);
                db.ShippingMethods.AddRange(shippingMethodsToAdd);
                foreach (var currency in seller.Currencies.Except(currenciesToAdd))
                {
                    db.Entry(currency).State = EntityState.Modified;
                }
                foreach (var address in seller.Addresses.Except(addressesToAdd))
                {
                    db.Entry(address).State = EntityState.Modified;
                } 
                foreach (var shipping in seller.ShippingMethods.Except(shippingMethodsToAdd))
                {
                    db.Entry(shipping).State = EntityState.Modified;
                }
                //schedules
                seller.Schedules.ForEach(entry => entry.SellerId = sellerId);
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
                var logo = Request.Files[0];
                if (logo != null && logo.ContentLength != 0)
                {
                    var sellerLogo = Request.Files[0];
                    var fileName = Path.GetFileName(sellerLogo.FileName);
                    var dotIndex = fileName.IndexOf('.');
                    var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
                    var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
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
                    db.SaveChanges();
                    db.Images.Add(img);
                    sellerLogo.SaveAs(Path.Combine(pathString, img.ImageUrl));
                    var imagesService = new ImagesService();
                    imagesService.ResizeToSiteRatio(Path.Combine(pathString, img.ImageUrl), ImageType.SellerLogo);
                }
                db.SaveChanges();
                return RedirectToAction("CreateOrUpdate", new { id = seller.Id });
            }

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
            var seller = db.Sellers.Find(id);
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
    }
}
