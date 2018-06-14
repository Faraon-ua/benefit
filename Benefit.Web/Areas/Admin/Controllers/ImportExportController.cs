using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
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
        ImportExportService ImportService = new ImportExportService();
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
                import.FileUrl = exportImport.FileUrl;
                import.SyncPeriod = exportImport.SyncPeriod;
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = "Дані імпорту оновлено";
            return RedirectToAction("Index");
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

        public ActionResult UploadExcelFile(string sellerUrlName, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Невірно вибраний файл";
            }
            else
            {
                var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                var ftpDirectory = new DirectoryInfo(originalDirectory).FullName;
                var sellerPath = Path.Combine(ftpDirectory, "FTP", "LocalUser", sellerUrlName);
                if (!Directory.Exists(sellerPath))
                {
                    Directory.CreateDirectory(sellerPath);
                }
                file.SaveAs(Path.Combine(sellerPath, "import.xls"));
            }
            TempData["SuccessMessage"] = "Файл успішно завантажено, тепер можна застосувати імпорт";

            return RedirectToAction("Index");
        }
    }
}