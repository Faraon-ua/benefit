using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Common.Helpers;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models.Enums;
using Benefit.Domain.Models.ModelExtensions;
using Benefit.Services;
using Benefit.Services.Domain;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Models.Admin;
using Microsoft.AspNet.Identity;
using WebGrease.Css.Extensions;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.OrdersManagerRoleName + ", " + DomainConstants.AdminRoleName + ", " + DomainConstants.SellerRoleName + ", " + DomainConstants.SellerModeratorRoleName)]
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
        public ActionResult Index(ProductFilterValues filters)
        {
            var resultProducts = new List<Product>();
            int resultsCount = 0;
            if (filters.HasValues || Seller.CurrentAuthorizedSellerId != null)
            {
                IQueryable<Product> products = db.Products.AsQueryable();
                if (!string.IsNullOrEmpty(filters.CategoryId))
                {
                    var category = db.Categories.FirstOrDefault(entry => entry.Id == filters.CategoryId);
                    var children = category.GetAllChildrenRecursively().ToList();
                    children.Add(category);
                    var categoryIds = children.Select(cat => cat.Id).ToList();
                    products = products.Where(entry => categoryIds.Contains(entry.CategoryId));
                }
                if (!string.IsNullOrEmpty(filters.SellerId))
                {
                    products = products.Where(entry => entry.SellerId == filters.SellerId);
                }
                if (Seller.CurrentAuthorizedSellerId != null)
                {
                    products = products.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId);
                }
                if (filters.IsAvailable)
                {
                    products = products.Where(entry => entry.AvailableAmount == null || entry.AvailableAmount > 0);
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
                        case ProductSortOption.Order:
                            products = products.OrderBy(entry => entry.Order);
                            break;
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
                            products = products.OrderByDescending(entry => entry.SKU);
                            break;
                        case ProductSortOption.PriceAsc:
                            products = products.OrderBy(entry => entry.Price);
                            break;
                        case ProductSortOption.PriceDesc:
                            products = products.OrderByDescending(entry => entry.Price);
                            break;
                    }
                }
                else
                {
                    products = products.OrderBy(entry => entry.Name);
                }
                resultsCount = products.Count();
                var skip = filters.Page > 0 ? ListConstants.DefaultTakePerPage * (filters.Page - 1) : 0;
                resultProducts = products.Skip(skip).Take(ListConstants.DefaultTakePerPage).ToList();
            }

            var productsViewModel = new ProductsViewModel()
            {
                Products = resultProducts,
                ProductFilters = new ProductFilters
                {
                    Sorting = (from ProductSortOption sortOption in Enum.GetValues(typeof(ProductSortOption))
                               select new SelectListItem() { Text = Enumerations.GetEnumDescription(sortOption), Value = sortOption.ToString(), Selected = sortOption == filters.Sorting }).ToList(),
                    Search = filters.Search,
                    PagesCount = (resultsCount / ListConstants.DefaultTakePerPage) % ListConstants.DefaultTakePerPage == 0 ? (resultsCount / ListConstants.DefaultTakePerPage) : (resultsCount / ListConstants.DefaultTakePerPage) + 1
                }
            };
            if (Seller.CurrentAuthorizedSellerId != null)
            {
                productsViewModel.ProductFilters.Categories =
                    db.Categories.Where(
                        entry =>
                            entry.SellerCategories.Where(sc => !sc.IsDefault).Select(sc => sc.SellerId).Contains(Seller.CurrentAuthorizedSellerId))
                        .OrderBy(entry => entry.ParentCategoryId)
                        .ThenBy(entry => entry.Name)
                        .ToList()
                        .Select(entry => new SelectListItem { Text = entry.ExpandedName, Value = entry.Id });
            }
            else
            {
                productsViewModel.ProductFilters.Sellers =
                   db.Sellers.OrderBy(entry => entry.Name)
                       .Select(entry => new SelectListItem { Text = entry.Name, Value = entry.Id });
                productsViewModel.ProductFilters.Categories =
                    db.Categories.Where(entry => !entry.IsSellerCategory).OrderBy(entry => entry.ParentCategoryId).ThenBy(entry => entry.Name).ToList()
                        .Select(entry => new SelectListItem { Text = entry.ExpandedName, Value = entry.Id });
            }
            return PartialView(productsViewModel);
        }

        public ActionResult CreateOrUpdate(string id)
        {
            //todo: add check for seller role
            var product = db.Products.Include("Category").Include(entry => entry.Category.ProductParameters).FirstOrDefault(entry => entry.Id == id) ??
                          new Product()
                          {
                              Id = Guid.NewGuid().ToString(),
                              IsActive = true,
                              DoesCountForShipping = true
                          };
            if (User.IsInRole(DomainConstants.AdminRoleName))
            {
                ViewBag.CategoryId = new SelectList(db.Categories, "Id", "ExpandedName", product.CategoryId);
            }
            else
            {
                var categories = db.Categories.Where(
                    entry =>
                        entry.SellerCategories.Where(sc => !sc.IsDefault)
                            .Select(sc => sc.SellerId)
                            .Contains(Seller.CurrentAuthorizedSellerId)).ToList();
                if (id != null)
                {
                    categories =
                        categories.Union(
                            db.Categories.Where(entry => entry.IsSellerCategory && entry.SellerId == product.SellerId)).ToList();
                }
                ViewBag.CategoryId = new SelectList(categories, "Id", "ExpandedName", product.CategoryId);
            }
            ViewBag.SellerId = new SelectList(db.Sellers, "Id", "Name", product.SellerId);
            var currencies =
                db.Currencies.Where(entry => entry.Provider == CurrencyProvider.PrivatBank || entry.SellerId == product.SellerId)
                    .OrderBy(entry => entry.Id).ToList();
            ViewBag.CurrencyId = new SelectList(currencies, "Id", "ExpandedName", product.CurrencyId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreateOrUpdate(Product product)
        {
            if (db.Products.Any(entry => entry.UrlName == product.UrlName && entry.Id != product.Id))
            {
                ModelState.AddModelError("UrlName", "Товар з такою Url назвою вже існує");
            }
            if (product.AvailableAmount < 0)
            {
                ModelState.AddModelError("AvailableAmount", "Доступна кількість не може бути негативного значення");
            }
            if (string.IsNullOrEmpty(product.ShortDescription))
            {
                ModelState.AddModelError("ShortDescription", "Короткий опис обовязковий для заповнення");
            }
            if (ModelState.IsValid)
            {
                if (Seller.CurrentAuthorizedSellerId != null)
                {
                    product.SellerId = Seller.CurrentAuthorizedSellerId;
                }
                product.LastModified = DateTime.UtcNow;
                product.LastModifiedBy = User.Identity.Name;
                product.ProductParameterProducts.ForEach(entry => entry.ProductId = product.Id);
                if (db.Products.Any(entry => entry.Id == product.Id))
                {
                    db.Entry(product).State = EntityState.Modified;
                }
                else
                {
                    var maxSku = db.Products.Max(entry => (int?)entry.SKU);
                    if (maxSku == null || maxSku < SettingsService.SkuMinValue)
                    {
                        maxSku = SettingsService.SkuMinValue;
                    }
                    product.SKU = (int)maxSku + 1;
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
            var ownerId = User.Identity.GetUserId();
            var seller = db.Sellers.FirstOrDefault(entry => ownerId == entry.Owner.Id) ?? db.Sellers.FirstOrDefault(entry => entry.Id == product.SellerId);
            if (User.IsInRole(DomainConstants.AdminRoleName))
            {
                ViewBag.CategoryId = new SelectList(db.Categories, "Id", "ExpandedName", product.CategoryId);
            }
            else
            {
                ViewBag.CategoryId = new SelectList(db.Categories.Where(
                    entry =>
                        entry.SellerCategories.Where(sc => !sc.IsDefault)
                            .Select(sc => sc.SellerId)
                            .Contains(Seller.CurrentAuthorizedSellerId)), "Id", "ExpandedName", product.CategoryId);
            }
            ViewBag.SellerId = new SelectList(db.Sellers, "Id", "Name");
            var resultCurrencies =
                db.Currencies.Where(entry => entry.Provider == CurrencyProvider.PrivatBank)
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
            var productService = new ProductsService();
            productService.Delete(id);
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

        public ActionResult UpdateWeightProduct(string id, bool isWeight)
        {
            var product = db.Products.Find(id);
            product.IsWeightProduct = isWeight;
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();
            return Json("success");
        }
    }
}
