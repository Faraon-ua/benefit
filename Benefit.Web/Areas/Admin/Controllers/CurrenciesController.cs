using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Action = Antlr.Runtime.Misc.Action;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class CurrenciesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View(db.Currencies.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId || entry.Provider == CurrencyProvider.PrivatBank).ToList());
        }

        [HttpGet]
        public ActionResult CreateOrUpdate(string id)
        {
            var currency = db.Currencies.FirstOrDefault(entry => entry.Id == id) ?? new Currency();
            if (currency != null)
            {
                var seller =
                    db.Sellers.Include(entry => entry.SellerCategories.Select(sc => sc.Category))
                        .Include(entry => entry.MappedCategories)
                        .FirstOrDefault(entry => entry.Id == Seller.CurrentAuthorizedSellerId);
                var cats =
                    seller.SellerCategories.Select(entry => entry.Category).Union(seller.MappedCategories).ToList();
                ViewBag.Categories =
                    cats.Select(entry => new SelectListItem() {Text = entry.Name, Value = entry.Id}).ToList();
            }
            return View(currency);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrUpdate(Currency currency)
        {
            if (currency.Id == null)
            {
                currency.Id = Guid.NewGuid().ToString();
                currency.SellerId = Seller.CurrentAuthorizedSellerId;
            }
            if (ModelState.IsValid)
            {
                if (db.Currencies.Any(entry => entry.Id == currency.Id))
                {
                    db.Entry(currency).State = EntityState.Modified;
                }
                else
                {
                    db.Currencies.Add(currency);
                }
                db.SaveChanges();
                TempData["SuccessMessage"] = "Курс/індекс збережено";
                return RedirectToAction("Index");
            }
            return View(currency);
        }

        public ActionResult ApplyToCats(string id, string[] catIds)
        {
            var products = db.Products.Where(entry => catIds.Contains(entry.CategoryId)).ToList();
            foreach (var product in products)
            {
                product.CurrencyId = id;
            }
            db.SaveChanges();
            return new HttpStatusCodeResult(200);
        }

        public ActionResult Delete(string id)
        {
            var currency = db.Currencies.Find(id);
            db.Currencies.Remove(currency);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Курс/індекс видалено";
            return RedirectToAction("Index");
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
