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
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.XmlModels;
using Benefit.Services;
using Benefit.Services.Domain;
using Benefit.Web.Models.Admin;
using NLog;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.AdminRoleName + ", " + DomainConstants.SellerRoleName + ", " + DomainConstants.SellerModeratorRoleName)]
    public class ImportExportController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        ProductsService ProductService = new ProductsService();
        EmailService EmailService = new EmailService();
        ImagesService ImagesService = new ImagesService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            var seller = db.Sellers.FirstOrDefault(entry => entry.Id == Seller.CurrentAuthorizedSellerId);
            return View(seller);
        }

        public ActionResult GetPromImport()
        {
            var importTask =
                db.ExportImports.FirstOrDefault(
                    entry => entry.SellerId == Seller.CurrentAuthorizedSellerId && entry.SyncType == SyncType.Promua) ?? new ExportImport()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IsActive = true,
                        IsImport = true,
                        SyncType = SyncType.Promua,
                        SellerId = Seller.CurrentAuthorizedSellerId
                    };
            var deleteProductsOption = new List<SelectListItem>()
            {
                new SelectListItem() {Text = "Видаляти", Value = true.ToString()},
                new SelectListItem() {Text = "Робити неактивними", Value = false.ToString()}
            };
            ViewBag.RemoveProducts = new SelectList(deleteProductsOption, "Value", "Text", importTask.RemoveProducts);

            var updateFrequency = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "Щоденно", Value = 1.ToString()},
                new SelectListItem() { Text = "Раз в 3 дні", Value = 3.ToString()},
                new SelectListItem() { Text = "Раз в тиждень", Value = 7.ToString()},
                new SelectListItem() { Text = "Раз в місяць", Value = 30.ToString()}
            };
            ViewBag.SyncPeriod = new SelectList(updateFrequency, "Value", "Text", importTask.SyncPeriod.ToString());
            return PartialView("_Promua", importTask);
        }

        [HttpPost]
        public ActionResult CreateOrUpdate(ExportImport exportImport)
        {
            if (exportImport.FileUrl == null)
            {
                TempData["ErrorMessage"] = "Url має бути заповнено";
                return RedirectToAction("Index");
            }
            if (db.ExportImports.Any(entry => entry.FileUrl == exportImport.FileUrl && entry.Id != exportImport.Id))
            {
                TempData["ErrorMessage"] = "Такий файл вже зареєстровано";
                return RedirectToAction("Index");
            }
            else
            {
                var import = db.ExportImports.Find(exportImport.Id);
                if (import == null)
                {
                    import = exportImport;
                    db.ExportImports.Add(import);
                }
                else
                {
                    db.Entry(import).State = EntityState.Modified;
                }
                import.IsActive = exportImport.IsActive;
                import.RemoveProducts = exportImport.RemoveProducts;
                import.FileUrl = exportImport.FileUrl;
                import.SyncPeriod = exportImport.SyncPeriod;
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = "Дані імпорту оновлено";
            return RedirectToAction("Index");
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
                    return Json(new { message = "Категорії постачалника на сайті мають повтори в назві" });
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
                return Json("Помилка імпорту файлу: " + ex.InnerException.Message);
            }
        }
    }
}