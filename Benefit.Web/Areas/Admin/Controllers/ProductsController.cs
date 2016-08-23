using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Web.Models.Admin;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Admin/Products/
        public ActionResult Index()
        {
            var productOptions = new ProductFilters
            {
                Categories =
                    db.Categories.OrderBy(entry => entry.ParentCategoryId).ThenBy(entry => entry.Name).ToList()
                        .Select(entry => new SelectListItem { Text = entry.ExpandedName, Value = entry.Id }),
                Sellers =
                    db.Sellers.OrderBy(entry => entry.Name)
                        .Select(entry => new SelectListItem { Text = entry.Name, Value = entry.Id })
            };
            return View(productOptions);
        }
        public ActionResult ProductsSearch(ProductFilterValues filters)
        {
            IQueryable<Product> products = db.Products.AsQueryable();
            if (!string.IsNullOrEmpty(filters.CategoryId))
            {
                products = products.Where(entry => entry.CategoryId == filters.CategoryId);
            }
            if (!string.IsNullOrEmpty(filters.SellerId))
            {
                products = products.Where(entry => entry.SellerId == filters.SellerId);
            }
            if (!string.IsNullOrEmpty(filters.Search))
            {
                filters.Search = filters.Search.ToLower();
                products = products.Where(entry => entry.SKU.ToString().Contains(filters.Search) ||
                                                   entry.Name.ToString().Contains(filters.Search));
            }

            return PartialView("_ProductsSearch", products);
        }

        public ActionResult CreateOrUpdate(string id)
        {
            var product = db.Products.Find(id) ??
                          new Product()
                          {
                              Id = Guid.NewGuid().ToString()
                          };
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "ExpandedName", product.CategoryId);
            ViewBag.SellerId = new SelectList(db.Sellers, "Id", "Name", product.SellerId);
            ViewBag.CurrencyId = new SelectList(db.Currencies.Where(entry=>entry.Provider == Common.Constants.DomainConstants.DefaultUSDCurrencyProvider).OrderBy(entry=>entry.Id), "Id", "ExpandedName", product.CurrencyId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreateOrUpdate(Product product)
        {
            if (ModelState.IsValid)
            {
                product.LastModified = DateTime.UtcNow;
                product.LastModifiedBy = User.Identity.Name;
                if (db.Products.Any(entry => entry.Id == product.Id))
                {
                    db.Entry(product).State = EntityState.Modified;
                }
                else
                {
                    product.SKU = (db.Products.Max(entry => (int?)entry.SKU) ?? SettingsService.SkuMinValue) + 1;
                    db.Products.Add(product);
                }
                db.SaveChanges();
                TempData["SuccessMessage"] = "Товар збережено";
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "ExpandedName");
            ViewBag.SellerId = new SelectList(db.Sellers, "Id", "Name");
            return View(product);
        }
        [HttpPost]
        public ActionResult LockUnlock(string id)
        {
            var product = db.Products.FirstOrDefault(entry => entry.Id == id);
            product.IsActive = !product.IsActive;
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();
            return Json(product.IsActive);
        }
        [HttpPost]
        public ActionResult Delete(string id)
        {
            var product = db.Products.Find(id);
            db.Products.Remove(product);
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
