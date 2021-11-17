using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Services.Domain;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.AdminRoleName + ", " + DomainConstants.SellerRoleName + ", " + DomainConstants.SellerModeratorRoleName)]
    public class ImportExportController : Controller
    {
        ProductsService ProductService = new ProductsService();
        EmailService EmailService = new EmailService();
        ImagesService ImagesService = new ImagesService();
        ExportService ImportService = new ExportService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers.FirstOrDefault(entry => entry.Id == Seller.CurrentAuthorizedSellerId);
                return View(seller);
            }
        }

        [HttpGet]
        public ActionResult GetImportTaskStatus(string sellerId, SyncType type)
        {
            using (var db = new ApplicationDbContext())
            {
                var importTask = db.ExportImports.FirstOrDefault(entry => entry.SellerId == sellerId && entry.SyncType == type);
                if (importTask != null)
                {
                    if (importTask.IsImport)
                    {
                        return Json(new { status = (int)ImportStatus.Pending }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { status = (importTask.LastUpdateStatus.Value ? 1 : 0), message = importTask.LastUpdateMessage }, JsonRequestBehavior.AllowGet);
                }
                return new HttpNotFoundResult();
            }
        }

        public ActionResult Status()
        {
            using (var db = new ApplicationDbContext())
            {
                var importTasks = db.ExportImports
                    .Include(entry => entry.Seller)
                    .Where(entry => entry.SellerId != null)
                    .OrderByDescending(entry => entry.IsActive)
                    .ThenByDescending(entry => entry.LastSync).ToList();
                foreach (var task in importTasks)
                {
                    if (task.IsActive)
                    {
                        if (DateTime.Now - task.LastSync < TimeSpan.FromDays(3) && task.LastUpdateStatus.GetValueOrDefault(false))
                        {
                            task.Status = ImportStatus.Success;
                        }
                        else
                        {
                            task.Status = ImportStatus.Error;
                        }
                    }
                }
                return View(importTasks);
            }
        }

        public ActionResult Exports()
        {
            using (var db = new ApplicationDbContext())
            {
                var exports = db.ExportImports
                .Include(entry => entry.Seller)
                .Where(entry => entry.SyncType == SyncType.YmlExport).ToList();

                return View(exports);
            }
        }

        public ActionResult CreateOrUpdateExport(string name)
        {
            using (var db = new ApplicationDbContext())
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
        }

        public ActionResult DeleteExport(string id)
        {
            using (var db = new ApplicationDbContext())
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
        }

        public ActionResult GetImportLinkForm(int number, string value)
        {
            return PartialView("_LinkForm", new KeyValuePair<int, string>(number, value));
        }

        public ActionResult GetImportForm(SyncType syncType)
        {
            using (var db = new ApplicationDbContext())
            {
                var newId = Guid.NewGuid().ToString();
                var importTask =
                    db.ExportImports
                    .Include(entry => entry.Links)
                    .FirstOrDefault(
                        entry => entry.SellerId == Seller.CurrentAuthorizedSellerId && entry.SyncType == syncType) ?? new ExportImport()
                        {
                            Id = newId,
                            IsActive = false,
                            IsImport = false,
                            SyncType = syncType,
                            FileUrl = string.Format("https://benefit.ua/export?id={0}", newId),
                            SellerId = Seller.CurrentAuthorizedSellerId,
                            Links = new List<Link>()
                        };

                var updateFrequency = new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "Щоденно", Value = 1.ToString()},
                    new SelectListItem() { Text = "Раз в 3 дні", Value = 3.ToString()},
                    new SelectListItem() { Text = "Раз в тиждень", Value = 7.ToString()},
                    new SelectListItem() { Text = "Раз в місяць", Value = 30.ToString()}
                };
                ViewBag.SyncPeriod = new SelectList(updateFrequency, "Value", "Text", importTask.SyncPeriod.ToString());
                var currencies =
                   db.Currencies.Where(entry => entry.Provider == CurrencyProvider.PrivatBank).OrderBy(entry => entry.Id).ToList();
                ViewBag.DefaultCurrencyId = new SelectList(currencies, "Id", "ExpandedName", importTask.DefaultCurrencyId);
                return PartialView("_ImportForm", importTask);
            }
        }

        [HttpPost]
        public ActionResult CreateOrUpdate(ExportImport exportImport)
        {
            using (var db = new ApplicationDbContext())
            {
                if (exportImport.FileUrl == null)
                {
                    TempData["ErrorMessage"] = "Url має бути заповнено";
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
                    import.IsImport = exportImport.IsImport;
                    import.IsActive = exportImport.IsActive;
                    import.FileUrl = exportImport.FileUrl;
                    import.SyncPeriod = exportImport.SyncPeriod;
                    import.DefaultCurrencyId = exportImport.DefaultCurrencyId;

                    db.Links.RemoveRange(db.Links.Where(entry => entry.ExportImportId == import.Id));
                    db.SaveChanges();
                    if (exportImport.Links != null && exportImport.Links.Any())
                    {
                        foreach (var link in exportImport.Links)
                        {
                            link.Id = Guid.NewGuid().ToString();
                            link.ExportImportId = import.Id;
                            db.Links.Add(link);
                        }
                    }
                }
                db.SaveChanges();
                TempData["SuccessMessage"] = "Дані імпорту/експорту оновлено";
                return RedirectToAction("Index");
            }
        }

        public ActionResult UploadExcelFile(string sellerUrlName, HttpPostedFileBase import, HttpPostedFileBase images)
        {
            using (var db = new ApplicationDbContext())
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
        }
    }
}