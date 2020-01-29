using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.XmlModels;
using Benefit.Services;
using Benefit.Services.Domain;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.AdminRoleName + ", " + DomainConstants.SellerRoleName + ", " + DomainConstants.SellerModeratorRoleName)]
    public class ImportExportController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        ProductsService ProductService = new ProductsService();
        EmailService EmailService = new EmailService();
        ImagesService ImagesService = new ImagesService();
        ImportExportService ImportService = new ImportExportService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            var seller = db.Sellers.FirstOrDefault(entry => entry.Id == Seller.CurrentAuthorizedSellerId);
            return View(seller);
        }

        public ActionResult Status()
        {
            var importTasks = db.ExportImports.Include(entry=>entry.Seller).Where(entry=>entry.SellerId != null).ToList();
            foreach(var task in importTasks)
            {
                if(DateTime.Now - task.LastSync > TimeSpan.FromDays(7))
                {
                    task.Status = 2;
                }
                if (DateTime.Now - task.LastSync > TimeSpan.FromDays(1) && DateTime.Now - task.LastSync < TimeSpan.FromDays(7))
                {
                    task.Status = 1;
                }
            }
            importTasks = importTasks.OrderByDescending(entry => entry.Status).ThenBy(entry=>entry.LastSync).ToList();
            return View(importTasks);
        }

        public ActionResult Exports()
        {
            var exports = db.ExportImports
                .Include(entry => entry.Seller)
                .Where(entry => entry.SyncType == SyncType.YmlExport).ToList();

            return View(exports);
        }

        public ActionResult CreateOrUpdateExport(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                TempData["ErrorMessage"] = "Назва не може бути порожньою";
            }
            else
            {
                var export = new ExportImport()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    SyncType = SyncType.YmlExport,
                    IsActive = true
                };
                db.ExportImports.Add(export);
                db.SaveChanges();
            }
            return RedirectToAction("Exports");
        }

        public ActionResult DeleteExport(string id)
        {
            var export = db.ExportImports.Include(entry => entry.ExportProducts).FirstOrDefault(entry => entry.Id == id);
            if (export != null)
            {
                db.ExportProducts.RemoveRange(export.ExportProducts);
                db.ExportCategories.RemoveRange(export.ExportCategories);
                db.ExportImports.Remove(export);
            }
            db.SaveChanges();
            return RedirectToAction("Exports");
        }

        public ActionResult GetImportForm(SyncType syncType)
        {
            var newId = Guid.NewGuid().ToString();
            var importTask =
                db.ExportImports.FirstOrDefault(
                    entry => entry.SellerId == Seller.CurrentAuthorizedSellerId && entry.SyncType == syncType) ?? new ExportImport()
                    {
                        Id = newId,
                        IsActive = false,
                        IsImport = false,
                        SyncType = syncType,
                        FileUrl = string.Format("https://benefit-company.com/export?id={0}", newId),
                        SellerId = Seller.CurrentAuthorizedSellerId
                    };

            var updateFrequency = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "Щоденно", Value = 1.ToString()},
                new SelectListItem() { Text = "Раз в 3 дні", Value = 3.ToString()},
                new SelectListItem() { Text = "Раз в тиждень", Value = 7.ToString()},
                new SelectListItem() { Text = "Раз в місяць", Value = 30.ToString()}
            };
            ViewBag.SyncPeriod = new SelectList(updateFrequency, "Value", "Text", importTask.SyncPeriod.ToString());
            return PartialView("_ImportForm", importTask);
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
                import.FileUrl = exportImport.FileUrl;
                import.SyncPeriod = exportImport.SyncPeriod;
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = "Дані імпорту/експорту оновлено";
            return RedirectToAction("Index");
        }

        public ActionResult YmlImport(string sellerId)
        {
            var importTask = db.ExportImports
                .AsNoTracking()
                .Include(entry => entry.Seller)
                .FirstOrDefault(entry => entry.SellerId == sellerId);
            if (importTask == null)
            {
                return Json(new { error = "Дані іморту ще не задано" }, JsonRequestBehavior.AllowGet);
            }

            var importExportService = new ImportExportService();
            Task.Run(() => importExportService.ImportFromYml(importTask.Id));
            return Json(new { message = "Іморт запущено" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ExceleImport(string id)
        {
            try
            {
                var seller = db.Sellers.Find(id);
                var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                var ftpDirectory = new DirectoryInfo(originalDirectory).FullName;
                var sellerPath = Path.Combine(ftpDirectory, "FTP", "LocalUser", seller.UrlName);
                var importFile = new DirectoryInfo(sellerPath).GetFiles("import.xls", SearchOption.AllDirectories).FirstOrDefault();

                if (importFile == null || importFile.Length == 0)
                {
                    return Json(new { error = "Файл import.xml не знайдено" }, JsonRequestBehavior.AllowGet);
                }
                var result = await ImportService.ImportFromExcel(id, importFile.FullName);

                Task.Run(() => EmailService.SendImportResults(seller.Owner.Email, result));
            }
            catch (Exception ex)
            {
                _logger.Fatal("[Excele Import] " + ex);
                return Json(new { error = "Файл імпорту має невірну структуру" });
            }

            return Json(new
            {
                message = "Імпорт з файлу Excel успішно виконаний"
            });

        }

        [HttpPost]
        public async Task<JsonResult> OneCCommerceMLImport(string id)
        {
            var seller = db.Sellers.Find(id);
            if (seller == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json("Постачальника не знайдено");
            }
            try
            {
                var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                var ftpDirectory = new DirectoryInfo(originalDirectory).FullName;
                var sellerPath = Path.Combine(ftpDirectory, "FTP", "LocalUser", seller.UrlName);
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
                if (!ImportService.ImportFrom1C(xml, seller))
                {
                    return Json(new { error = "Ошибка імпорту файлів" }, JsonRequestBehavior.AllowGet);
                }

                //Task.Run(() => EmailService.SendImportResults(seller.Owner.Email, results));

                //images
                xml = XDocument.Load(offersFile.FullName);

                var xmlProductPrices = xml.Descendants("Предложение");
                var poductPrices = xmlProductPrices.Select(entry => new XmlProductPrice(entry)).ToList();
                var pricesResult = ProductService.ProcessImportedProductPrices(poductPrices);

                return Json(new
                {
                    message = "Імпорт 1C Commerce ML успішно виконаний"
                });

            }
            catch (XmlException)
            {
                return Json(new { errror = "Завантажений файл має невірну структуру" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Json(new { error = "Помилка імпорту файлу: " + ex }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UploadExcelFile(string sellerUrlName, HttpPostedFileBase import, HttpPostedFileBase images)
        {
            if (import == null || import.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Невірно вибраний файл імпорту";
            }
            else
            {
                var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                var ftpDirectory = new DirectoryInfo(originalDirectory).FullName;
                var sellerPath = Path.Combine(ftpDirectory, "FTP", "LocalUser", sellerUrlName);
                var imagesPath = Path.Combine(sellerPath, "images");
                if (!Directory.Exists(sellerPath))
                {
                    Directory.CreateDirectory(sellerPath);
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }
                }
                import.SaveAs(Path.Combine(sellerPath, "import.xls"));
                if (images != null && images.ContentLength > 0)
                {
                    var imagesFile = Path.Combine(sellerPath, "images.zip");
                    images.SaveAs(imagesFile);
                    try
                    {
                        var filePaths = Directory.GetFiles(imagesPath);
                        foreach (var filePath in filePaths)
                        {
                            System.IO.File.Delete(filePath);
                        }

                        ZipFile.ExtractToDirectory(imagesFile, imagesPath);
                        System.IO.File.Delete(imagesFile);
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Не вдалось розархівувати файл із зображеннями";
                    }
                }
            }
            TempData["SuccessMessage"] = "Файли успішно завантажено, тепер можна застосувати імпорт";

            return RedirectToAction("Index");
        }

        public ActionResult YmlImportStatus(string sellerId)
        {
            var importTask = db.ExportImports.FirstOrDefault(entry => entry.SellerId == sellerId);
            return Json(new { status = (importTask != null && importTask.IsImport) }, JsonRequestBehavior.AllowGet);
        }
    }
}