using System;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Web.Areas.Admin.Controllers.Base;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class LocalizationsController : AdminController
    {
        LocalizationService LocalizationService = new LocalizationService();
        public ActionResult Index(string SellerId)
        {
            using (var db = new ApplicationDbContext())
            {
                var products = db.Products.Where(entry => entry.SellerId == SellerId).OrderBy(entry => entry.SKU).Skip(0).Take(50).ToList();
                var typeName = typeof(Product).ToString();
                var productIds = products.Select(pr => pr.Id).ToList();
                var localizations = db.Localizations.Where(entry => productIds.Contains(entry.ResourceId) && entry.ResourceType == typeName).ToList();
                foreach (var product in products)
                {
                    product.Localizations = localizations.Where(entry => entry.ResourceId == product.Id).ToList();
                }
                ViewBag.Sellers =
                    db.Sellers.Select(entry => new SelectListItem() { Text = entry.Name, Value = entry.Id }).ToList();
                return View(products);
            }
        }

        public ActionResult Export(string sellerId)
        {
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers.Find(sellerId);
                return File(LocalizationService.ExportProducts(sellerId), "text/csv",
                    string.Format("{0}-{1}.csv", seller.UrlName, DateTime.Now.ToShortDateString()));
            }
        }
        public ActionResult Import(string sellerId)
        {
            if (Request.Files.Count == 0)
            {
                TempData["ErrorMessage"] = "Не вибрано файл імпорту";
                return RedirectToAction("Index");
            }
            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Невірний файл імпорту";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}