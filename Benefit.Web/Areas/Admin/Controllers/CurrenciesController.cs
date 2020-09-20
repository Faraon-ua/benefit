using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class CurrenciesController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new ApplicationDbContext())
            {
                return View(db.Currencies.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId || entry.Provider == CurrencyProvider.PrivatBank).ToList());
            }
        }

        [HttpGet]
        public ActionResult CreateOrUpdate(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var currency = db.Currencies.FirstOrDefault(entry => entry.Id == id) ?? new Currency();
                var seller =
                    db.Sellers.Include(entry => entry.SellerCategories.Select(sc => sc.Category))
                        .Include(entry => entry.MappedCategories)
                        .FirstOrDefault(entry => entry.Id == Seller.CurrentAuthorizedSellerId);

                return View(currency);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrUpdate(Currency currency)
        {
            using (var db = new ApplicationDbContext())
            {
                currency.Provider = CurrencyProvider.Custom;
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
        }

        public ActionResult Delete(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var products = db.Products.Where(entry => entry.CurrencyId == id).ToList();
                if (products.Any())
                {
                    var defaultCurrency =
                        db.Currencies.First(entry => entry.Provider == CurrencyProvider.PrivatBank && entry.Name == "UAH");
                    foreach (var product in products)
                    {
                        product.CurrencyId = defaultCurrency.Id;
                    }
                }
                var currency = db.Currencies.Find(id);
                db.Currencies.Remove(currency);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Курс/індекс видалено";
                return RedirectToAction("Index");
            }
        }
    }
}
