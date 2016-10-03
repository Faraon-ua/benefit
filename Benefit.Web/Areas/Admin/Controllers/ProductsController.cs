using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Common.Helpers;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Models.Admin;
using WebGrease.Css.Extensions;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class ProductsController : AdminController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult GetProductGallery(string id)
        {
            var product = db.Products.Find(id);
            if (product != null)
                return Json(product.Images.Select(entry => new { entry.ImageUrl }), JsonRequestBehavior.AllowGet);
            return Json(null);
        }


        [HttpGet]
        public ActionResult Index(ProductFilterValues filters, int skip = 0, int take = ListConstants.DefaultTakePerPage)
        {
            var resultProducts = new List<Product>();
            if (filters.HasValues)
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
                if (filters.IsAvailable)
                {
                    products = products.Where(entry => entry.Amount == null || entry.Amount > 0);
                }
                if (!string.IsNullOrEmpty(filters.Search))
                {
                    filters.Search = filters.Search.ToLower();
                    products = products.Where(entry => entry.SKU.ToString().Contains(filters.Search) ||
                                                       entry.Name.ToString().Contains(filters.Search));
                }
                if (filters.Sorting.HasValue)
                {
                    switch (filters.Sorting.Value)
                    {
                        case ProductSortOption.NameAsc:
                            products = products.OrderBy(entry => entry.Name);
                            break;
                        case ProductSortOption.NameDesc:
                            products = products.OrderByDescending(entry => entry.Name);
                            break;
                        case ProductSortOption.SKUAsc:
                            products = products.OrderBy(entry => entry.SKU);
                            break;
                        case ProductSortOption.SKUDesc:
                            products = products.OrderByDescending(entry => entry.Name);
                            break;
                    }
                }

                resultProducts = products.OrderBy(entry => entry.SKU).Skip(skip).Take(take).ToList();
            }

            var productsViewModel = new ProductsViewModel()
            {
                Products = resultProducts,
                ProductFilters = new ProductFilters
                {
                    Categories =
                        db.Categories.OrderBy(entry => entry.ParentCategoryId).ThenBy(entry => entry.Name).ToList()
                            .Select(entry => new SelectListItem { Text = entry.ExpandedName, Value = entry.Id }),
                    Sellers =
                        db.Sellers.OrderBy(entry => entry.Name)
                            .Select(entry => new SelectListItem { Text = entry.Name, Value = entry.Id }),
                    Sorting = (from ProductSortOption sortOption in Enum.GetValues(typeof(ProductSortOption))
                               select new SelectListItem() { Text = Enumerations.GetEnumDescription(sortOption), Value = sortOption.ToString(), Selected = sortOption == filters.Sorting }).ToList(),
                    Search = filters.Search
                }
            };
            return PartialView(productsViewModel);
        }


        public ActionResult CreateOrUpdate(string id)
        {
            //todo: add check for seller role
            var product = db.Products.Include("Category").Include(entry=>entry.Category.ProductParameters).FirstOrDefault(entry=>entry.Id == id) ??
                          new Product()
                          {
                              Id = Guid.NewGuid().ToString()
                          };
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "ExpandedName", product.CategoryId);
            ViewBag.SellerId = new SelectList(db.Sellers, "Id", "Name", product.SellerId);
            var resultCurrencies =
                db.Currencies.Where(entry => entry.Provider == DomainConstants.DefaultUSDCurrencyProvider)
                    .OrderBy(entry => entry.Id).ToList();
            if (User.IsInRole(DomainConstants.SellerRoleName))
            {
                var seller = db.Sellers.FirstOrDefault(entry => User.Identity.Name == entry.Owner.UserName);
                var sellerCurrencies = db.Currencies.Where(entry => entry.SellerId == seller.Id);
                resultCurrencies.AddRange(sellerCurrencies);
            }
            ViewBag.CurrencyId = new SelectList(resultCurrencies, "Id", "ExpandedName", product.CurrencyId);
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
                product.ProductParameterProducts.ForEach(entry=>entry.ProductId = product.Id);
                if (db.Products.Any(entry => entry.Id == product.Id))
                {
                    db.Entry(product).State = EntityState.Modified;
                }
                else
                {
                    product.SKU = (db.Products.Max(entry => (int?)entry.SKU) ?? SettingsService.SkuMinValue) + 1;
                    db.Products.Add(product);
                }
                db.ProductParameterProducts.RemoveRange(
                    db.ProductParameterProducts.Where(entry => entry.ProductId == product.Id));
                db.ProductParameterProducts.AddRange(product.ProductParameterProducts);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Товар збережено";
                return RedirectToAction("CreateOrUpdate", new { id = product.Id });
            }
            //todo: add check for seller role
            var seller = db.Sellers.FirstOrDefault(entry => User.Identity.Name == entry.Owner.UserName);
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "ExpandedName");
            ViewBag.SellerId = new SelectList(db.Sellers, "Id", "Name");
            var resultCurrencies =
                db.Currencies.Where(entry => entry.Provider == DomainConstants.DefaultUSDCurrencyProvider)
                    .OrderBy(entry => entry.Id).ToList();
            var sellerCurrencies = db.Currencies.Where(entry => entry.SellerId == seller.Id);
            resultCurrencies.AddRange(sellerCurrencies);
            ViewBag.CurrencyId = new SelectList(resultCurrencies, "Id", "ExpandedName", product.CurrencyId);
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
