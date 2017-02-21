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
using Benefit.Common.Constants;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models.XmlModels;
using Benefit.Services;
using Benefit.Services.Domain;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Models.Admin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NLog;
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
        ImagesService ImagesService = new ImagesService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public SellersController()
        {
            var userStore = new UserStore<ApplicationUser>(db);
            UserManager = new UserManager<ApplicationUser>(userStore);
        }

        private List<XElement> GetAllFiniteCategories(IEnumerable<XElement> xmlCategories)
        {
            var resultXmlCategories = new List<XElement>();
            var hadChildren = false;
            foreach (var rawXmlCategory in xmlCategories)
            {
                if (rawXmlCategory.Element("Группы") != null)
                {
                    resultXmlCategories.AddRange(rawXmlCategory.Element("Группы").Elements());
                    hadChildren = true;
                }
                else
                {
                    resultXmlCategories.Add(rawXmlCategory);
                }
            }
            if (hadChildren)
            {
                resultXmlCategories = GetAllFiniteCategories(resultXmlCategories);
            }
            return resultXmlCategories;
        }

        public ActionResult DownloadIntelHexFile(string id)
        {
            var seller = db.Sellers.Find(id);
            var fileContent = SellerService.GenerateIntelHexFile(seller.TerminalPassword);
            return File(fileContent, System.Net.Mime.MediaTypeNames.Application.Octet, "IntelHexFile.hex");
        }

        [HttpPost]
        public async Task<JsonResult> OneCCommerceMLImport(string id)
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
                var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                var ftpDirectory = new DirectoryInfo(originalDirectory).Parent.FullName;
                var sellerPath = Path.Combine(ftpDirectory, "FTP", seller.UrlName);
                var importFile = new DirectoryInfo(sellerPath).GetFiles("import.xml", SearchOption.AllDirectories).FirstOrDefault();
                var offersFile = new DirectoryInfo(sellerPath).GetFiles("offers.xml", SearchOption.AllDirectories).FirstOrDefault();

                if (importFile == null || importFile.Length == 0)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json("Файл import.xml не знайдено");
                }
                if (offersFile == null || offersFile.Length == 0)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json("Файл offers.xml не знайдено");
                }
                
                var xml = XDocument.Load(importFile.FullName);

                var rawXmlCategories = xml.Descendants("Группы").First().Elements().ToList();
                var resultXmlCategories = GetAllFiniteCategories(rawXmlCategories);

                xmlCategories = resultXmlCategories.Select(entry => new XmlCategory(entry)).ToList();

                var sellerDbCategories =
                    seller.SellerCategories.Where(entry => !entry.IsDefault)
                        .Select(entry => entry.Category)
                        .ToList();

                try
                {
                    foreach (var dbCategory in sellerDbCategories)
                    {
                        var xmlCategory = xmlCategories.FirstOrDefault(entry => entry.Name == dbCategory.Name);
                        if (xmlCategory != null)
                        {
                            if (!xmlToDbCategoriesMapping.ContainsKey(xmlCategory.Id))
                            {
                                xmlToDbCategoriesMapping.Add(xmlCategory.Id, dbCategory.Id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new {message = "Категорії постачалника на сайті мають повтори в назві"});
                }

                xmlProducts =
                    xml.Descendants("Товары").First().Elements().Select(entry => new XmlProduct(entry)).ToList();
                xmlProducts =
                    xmlProducts.Where(
                        entry =>
                            xmlToDbCategoriesMapping.Keys.Contains(entry.CategoryId)).ToList();
                xmlProducts.ForEach(entry => entry.CategoryId = xmlToDbCategoriesMapping[entry.CategoryId]);
                results = ProductService.ProcessImportedProducts(xmlProducts, seller.Id, seller.UrlName);

                Task.Run(() => EmailService.SendImportResults(seller.Owner.Email, results));

                //images
                var filesPath =
                    new DirectoryInfo(sellerPath).GetDirectories("import_files", SearchOption.AllDirectories)
                        .FirstOrDefault();
                if (filesPath == null)
                {
                    return Json("Імпорт файлу import.xml успішно виконаний, але не було знайдено каталог зображень");
                }
                var ftpImagesPath = filesPath.Parent.FullName;

                var imageType = ImageType.ProductGallery;
                foreach (var xmlProduct in xmlProducts.Where(entry => !string.IsNullOrEmpty(entry.Image)))
                {
                    var destPath = Path.Combine(originalDirectory, "Images", imageType.ToString(), xmlProduct.Id);
                    var isExists = Directory.Exists(destPath);
                    if (!isExists)
                        Directory.CreateDirectory(destPath);

                    var ftpImage = new FileInfo(Path.Combine(ftpImagesPath, xmlProduct.Image));
                    if (ftpImage.Exists)
                    {
                        ImagesService.DeleteAll(
                            db.Images.Where(entry => entry.ProductId == xmlProduct.Id).ToList(), xmlProduct.Id,
                            imageType);
                        ftpImage.CopyTo(Path.Combine(destPath, ftpImage.Name), true);
                        ImagesService.AddImage(xmlProduct.Id, ftpImage.Name, imageType);
                    }
                }

                xml = XDocument.Load(offersFile.FullName);

                var xmlProductPrices = xml.Descendants("Предложение");
                var poductPrices = xmlProductPrices.Select(entry => new XmlProductPrice(entry)).ToList();
                var pricesResult = ProductService.ProcessImportedProductPrices(poductPrices);
                EmailService.SendPricesImportResults(seller.Owner.Email, pricesResult);

                return Json(new
                {
                    message = "Імпорт 1C Commerce ML успішно виконаний"
                });

            }
            catch (XmlException)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Завантажений файл має невірну структуру");
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                _logger.Error(ex);
                return Json("Помилка імпорту файлу: "+ex.InnerException.Message);
            }
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
            var existingSeller = db.Sellers.Include(entry => entry.Personnels).Include("Schedules").Include(entry => entry.ShippingMethods.Select(sp => sp.Region)).Include(entry => entry.SellerCategories.Select(sc => sc.Category)).FirstOrDefault(entry => entry.Id == id);
            var seller = new SellerViewModel()
            {
                Seller = existingSeller ?? new Seller() { Schedules = SetSellerSchedules().ToList() }
            };
            if (!seller.Seller.Schedules.Any())
            {
                seller.Seller.Schedules = SetSellerSchedules().ToList();
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
                    var oldOwner = db.Users.AsNoTracking().Include(entry=>entry.OwnedSellers).FirstOrDefault(entry => entry.Id == seller.OwnerId);
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
            var scIds = sellervm.Seller.SellerCategories.Select(sc => sc.CategoryId).ToList();
            sellervm.Seller.SellerCategories =
                db.SellerCategories.Where(
                    entry => scIds.Contains(entry.CategoryId) && entry.SellerId == sellervm.Seller.Id).ToList();
            sellervm.Seller.Personnels = db.Personnels.Where(entry => entry.SellerId == sellervm.Seller.Id).ToList();
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
            imagesService.DeleteAll(seller.Images, id, ImageType.SellerGallery, true, false);
            imagesService.DeleteAll(seller.Images, id, ImageType.SellerLogo, true, false);
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
