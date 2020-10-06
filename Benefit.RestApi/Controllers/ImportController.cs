using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.XmlModels;
using Benefit.Services.Domain;
using Benefit.Services.Import;
using NLog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace Benefit.RestApi.Controllers
{
    public class ImportController : Controller
    {
        ImportExportService ImportService = new ImportExportService();
        ProductsService ProductService = new ProductsService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        public ActionResult GetImportTaskStatus(string sellerId, SyncType type)
        {
            using (var db = new ApplicationDbContext())
            {
                var importTask = db.ExportImports.FirstOrDefault(entry => entry.SellerId == sellerId && entry.SyncType == SyncType.OneCCommerceMl);
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

        [HttpGet]
        public async Task OneCCommerceMLImport(string id)
        {
            await Task.Run(() =>
            {
                using (var db = new ApplicationDbContext())
                {
                    var seller = db.Sellers.Find(id);
                    var importTask = db.ExportImports.FirstOrDefault(entry => entry.SellerId == id && entry.SyncType == SyncType.OneCCommerceMl);
                    importTask.LastUpdateStatus = false;
                    importTask.IsImport = true;
                    db.SaveChanges();
                    if (seller == null || importTask == null)
                    {
                        importTask.LastUpdateMessage = "Постачальника не знайдено";
                    }
                    try
                    {
                        var xml = XDocument.Load(importTask.FileUrl);
                        if (!ImportService.ImportFrom1C(xml, seller))
                        {
                            importTask.LastUpdateMessage = "Ошибка імпорту файлів";
                        }

                        //Task.Run(() => EmailService.SendImportResults(seller.Owner.Email, results));

                        //images
                        xml = XDocument.Load(importTask.FileUrl.Replace("import", "offers"));

                        var xmlProductPrices = xml.Descendants("Предложение");
                        var poductPrices = xmlProductPrices.Select(entry => new XmlProductPrice(entry)).ToList();
                        var pricesResult = ProductService.ProcessImportedProductPrices(poductPrices);

                        importTask.LastUpdateStatus = true;
                        importTask.LastUpdateMessage = "Імпорт 1C Commerce ML успішно виконаний";
                    }
                    catch (XmlException)
                    {
                        importTask.LastUpdateMessage = "Завантажений файл має невірну структуру";
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        importTask.LastUpdateMessage = "Помилка імпорту файлу: " + ex.ToString();
                    }
                    finally
                    {
                        importTask.IsImport = false;
                        db.SaveChanges();
                    }
                }
            });
        }

        public ActionResult ProcessImport(string sellerId, SyncType type)
        {
            using (var db = new ApplicationDbContext())
            {
                var importTask = db.ExportImports
                .AsNoTracking()
                .Include(entry => entry.Seller)
                .FirstOrDefault(entry => entry.SellerId == sellerId && entry.SyncType == type);
                if (importTask == null)
                {
                    return Json(new { error = "Дані іморту ще не задано" }, JsonRequestBehavior.AllowGet);
                }

                if (type == SyncType.Yml)
                {
                    var importExportService = new ImportExportService();
                    Task.Run(() => importExportService.ImportFromYml(importTask.Id));
                }
                if (type == SyncType.Gbs)
                {
                    var gbsService = new GbsImportService();
                    Task.Run(() => gbsService.Import(importTask.Id));
                }
                return Json(new { message = "Іморт запущено" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}