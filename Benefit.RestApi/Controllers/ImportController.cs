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
        ApplicationDbContext db = new ApplicationDbContext();
        ImportExportService ImportService = new ImportExportService();
        ProductsService ProductService = new ProductsService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        [HttpPost]
        public async Task<JsonResult> OneCCommerceMLImport(string id)
        {
            var seller = db.Sellers.Find(id);
            var importTask = db.ExportImports.FirstOrDefault(entry => entry.SellerId == id && entry.SyncType == Domain.Models.SyncType.OneCCommerceMl);
            if (seller == null || importTask == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json("Постачальника не знайдено");
            }
            try
            {
                var xml = XDocument.Load(importTask.FileUrl);
                if (!ImportService.ImportFrom1C(xml, seller))
                {
                    return Json(new { error = "Ошибка імпорту файлів" }, JsonRequestBehavior.AllowGet);
                }

                //Task.Run(() => EmailService.SendImportResults(seller.Owner.Email, results));

                //images
                xml = XDocument.Load(importTask.FileUrl.Replace("import","offers"));

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

        public ActionResult ProcessImport(string sellerId, SyncType type)
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