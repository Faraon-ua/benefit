using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models.ModelExtensions;
using Benefit.Domain.Models.XmlModels;
using Benefit.Services;
using Benefit.Services.Domain;
using Benefit.Web.Areas.Admin.Controllers.Base;
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
        ProductsService ProductService = new ProductsService();
        EmailService EmailService = new EmailService();
        SellerService SellerService = new SellerService();

        public SellersController()
        {
            var userStore = new UserStore<ApplicationUser>(db);
            UserManager = new UserManager<ApplicationUser>(userStore);
        }

        [HttpPost]
        public async Task<JsonResult> Import1C(string id)
        {
            ProductImportResults results = null;

            List<XmlCategory> xmlCategories = null;
            List<XmlProduct> xmlProducts = null;
            var xmlToDbCategoriesMapping = new Dictionary<string, string>();

            var seller = db.Sellers.Find(id);
            if (seller == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json("Постачальника не знайдено");
            }
            try
            {
                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        // get a stream
                        var xml = XDocument.Load(fileContent.InputStream);
                        xmlCategories = xml.Descendants("Группы").First().Elements().Select(entry => new XmlCategory(entry)).ToList();

                        var defaultCategories = seller.SellerCategories.Where(entry => entry.IsDefault).Select(entry => entry.Category).ToList();
                        var allDbCategories = defaultCategories.SelectMany(entry => entry.GetAllChildrenRecursively()).Distinct().ToList();

                        foreach (var dbCategory in allDbCategories)
                        {
                            var xmlCategory = xmlCategories.FirstOrDefault(entry => entry.Name == dbCategory.Name);
                            if (xmlCategory != null)
                            {
                                xmlToDbCategoriesMapping.Add(xmlCategory.Id, dbCategory.Id);
                            }
                        }

                        xmlProducts = xml.Descendants("Товары").First().Elements().Select(entry => new XmlProduct(entry)).ToList();
                        xmlProducts =
                            xmlProducts.Where(
                                entry =>
                                    entry.Price.HasValue && xmlToDbCategoriesMapping.Keys.Contains(entry.CategoryId)).ToList();
                        xmlProducts.ForEach(entry => entry.CategoryId = xmlToDbCategoriesMapping[entry.CategoryId]);
                        results = ProductService.ProcessImportedProducts(xmlProducts, xmlToDbCategoriesMapping.Values, seller.Id);
                        EmailService.SendImportResults(seller.Owner.Email, results);
                    }
                }
            }
            catch (XmlException)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Завантажений файл має невірну структуру");
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Помилка імпорту файлу");
            }

            return Json(new
            {
                message = "Імпорт файлу успішно виконаний"
            });
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
            var cats = db.Categories.Include(entry => entry.ChildCategories).Where(entry => entry.ParentCategoryId == null).ToList();
            catsList.AddRange(cats);
            catsList.AddRange(cats.SelectMany(entry => entry.ChildCategories));
            catsList = catsList.OrderBy(entry => entry.ExpandedName).ToList();
            var options = new SellerFilterOptions
            {
                Categories = catsList.Select(entry => new SelectListItem() { Text = entry.ExpandedName, Value = entry.Id }).ToList(),
                PointRatio = SettingsService.DiscountPercentToPointRatio.Values.Distinct().Select(entry => new SelectListItem() { Text = entry + ":1", Value = entry.ToString() }).ToList(),
                TotalDiscountPercent = SettingsService.DiscountPercentToPointRatio.Keys.Select(entry => new SelectListItem() { Text = entry + " %", Value = entry.ToString() }).ToList(),
                UserDiscountPercent = SettingsService.RewardsPlan.UserDiscounts.Select(entry => new SelectListItem() { Text = entry + " %", Value = entry.ToString() }).ToList()
            };
            options.Categories.Insert(0, new SelectListItem() { Text = "Всі постачальники", Value = "all" });
            return View(options);
        }

        // GET: /Admin/Sellers/Create
        public ActionResult UpdateSellerByOwenrId(string ownerId)
        {
            var owner = db.Users.Find(ownerId);
            if (owner == null) return HttpNotFound();
            if (!owner.OwnedSellers.Any()) return HttpNotFound();
            return RedirectToAction("CreateOrUpdate", new { id = owner.OwnedSellers.First().Id });
        }

        public ActionResult CreateOrUpdate(string id = null)
        {
            var existingSeller = db.Sellers.Include("Schedules").Include(entry => entry.ShippingMethods.Select(sp => sp.Region)).Include(entry=>entry.SellerCategories.Select(sc=>sc.Category)).FirstOrDefault(entry => entry.Id == id);
            existingSeller.SellerCategories = existingSeller.SellerCategories.OrderBy(entry => entry.Category.Order).ToList();
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
                if (seller.OwnerId != owner.Id)
                {
                    //remove old owner a seller role if it was his only seller
                    if (owner.OwnedSellers.Count == 1)
                    {
                        UserManager.RemoveFromRole(seller.OwnerId, "Seller");
                    }
                    //add new owner a seller role
                    UserManager.AddToRole(owner.Id, "Seller");
                    seller.OwnerId = owner.Id;
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

                    seller.Currencies = seller.Currencies.Distinct(new CurrencyComparer()).ToList();
                    SellerService.ProcessCurrencies(seller.Currencies.ToList(), sellerId);

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
            var imagesService = new ImagesService();
            imagesService.DeleteAll(seller.Images, id, ImageType.SellerGallery);
            imagesService.DeleteAll(seller.Images, id, ImageType.SellerLogo);
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
