using Benefit.Common.Constants;
using Benefit.Common.Helpers;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;
using Benefit.Domain.Models.ModelExtensions;
using Benefit.Services;
using Benefit.Services.Domain;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Models;
using Benefit.Web.Models.Admin;
using Benefit.Web.Models.Enumerations;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
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
            {
                return Json(product.Images.Select(entry => new { entry.ImageUrl }), JsonRequestBehavior.AllowGet);
            }

            return Json(null);
        }

        [HttpGet]
        public ActionResult Index(ProductFilterValues filters)
        {
            var resultProducts = new List<Product>();
            int resultsCount = 0;
            if (filters.HasValues || Seller.CurrentAuthorizedSellerId != null)
            {
                var products = GetFilteredProducts(filters);
                resultsCount = products.Count();
                var skip = filters.Page > 0 ? ListConstants.DefaultTakePerPage * (filters.Page - 1) : 0;
                resultProducts = products.Skip(skip).Take(ListConstants.DefaultTakePerPage).ToList();
            }

            var productsViewModel = new ProductsViewModel()
            {
                TotalProductsCount = resultsCount,
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
                var categories = db.Categories.Where(
                    entry =>
                        entry.SellerCategories.Where(sc => !sc.IsDefault)
                            .Select(sc => sc.SellerId)
                            .Contains(Seller.CurrentAuthorizedSellerId))
                    .ToList();
                categories =
                    categories.Union(
                        db.Categories.Where(entry => entry.IsSellerCategory && entry.SellerId == Seller.CurrentAuthorizedSellerId)).ToList().SortByHierarchy().ToList();
                productsViewModel.ProductFilters.Categories =
                    categories.Select(entry => new HierarchySelectItem() { Text = entry.Name, Value = entry.Id, Level = entry.HierarchicalLevel }).ToList();
                productsViewModel.ProductFilters.Categories.Insert(0, new HierarchySelectItem() { Text = "Не обрано", Value = string.Empty, Level = 0 });
            }
            else
            {
                productsViewModel.ProductFilters.Sellers =
                   db.Sellers.OrderBy(entry => entry.Name)
                       .Select(entry => new SelectListItem { Text = entry.Name, Value = entry.Id });
                var cats = db.Categories.Where(entry => !entry.IsSellerCategory).ToList().SortByHierarchy().ToList();
                productsViewModel.ProductFilters.Categories = cats.Select(entry => new HierarchySelectItem() { Text = entry.Name, Value = entry.Id, Level = entry.HierarchicalLevel }).ToList();
                productsViewModel.ProductFilters.Categories.Insert(0, new HierarchySelectItem() { Text = "Не обрано", Value = string.Empty, Level = 1 });
            }

            productsViewModel.ProductFilters.Exports = db.ExportImports
                .Where(entry => entry.SyncType == SyncType.YmlExport).Select(entry => new SelectListItem()
                {
                    Text = entry.Name,
                    Value = entry.Id
                }).ToList();
            productsViewModel.ProductFilters.HasParameters = new List<SelectListItem>()
            {
                new SelectListItem() {Text = "Мають", Value = "true"},
                new SelectListItem() {Text = "Не мають", Value = "false"}
            };
            productsViewModel.ProductFilters.HasVendor = new List<SelectListItem>()
            {
                new SelectListItem() {Text = "Задано", Value = "true"},
                new SelectListItem() {Text = "Не задано", Value = "false"}
            };
            productsViewModel.ProductFilters.HasOriginCountry = new List<SelectListItem>()
            {
                new SelectListItem() {Text = "Задано", Value = "true"},
                new SelectListItem() {Text = "Не задано", Value = "false"}
            };
            productsViewModel.ProductFilters.IsAvailable = new List<SelectListItem>()
            {
                new SelectListItem() {Text = "В наявності", Value = "true"},
                new SelectListItem() {Text = "Немає в наявності", Value = "false"}
            };
            return PartialView(productsViewModel);
        }

        public ActionResult CreateOrUpdate(string id)
        {
            //todo: add check for seller role
            var product = db.Products
                              .Include(entry => entry.Category)
                              .Include(entry => entry.Category.ProductParameters)
                              .Include(entry => entry.Category.MappedParentCategory.ProductParameters)
                              .Include(entry => entry.Reviews)
                              .FirstOrDefault(entry => entry.Id == id) ??
                          new Product()
                          {
                              Id = Guid.NewGuid().ToString(),
                              IsActive = true,
                              DoesCountForShipping = true
                          };
            if (User.IsInRole(DomainConstants.AdminRoleName))
            {
                ViewBag.Categories = db.Categories.ToList().SortByHierarchy().ToList().Select(entry => new HierarchySelectItem()
                {
                    Text = entry.Name,
                    Value = entry.Id,
                    Level = entry.HierarchicalLevel
                });
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
                            db.Categories.Where(entry => entry.IsSellerCategory && entry.SellerId == product.SellerId)).ToList().SortByHierarchy().ToList();
                }

                ViewBag.Categories = categories.Select(entry => new HierarchySelectItem()
                {
                    Text = entry.Name,
                    Value = entry.Id,
                    Level = entry.HierarchicalLevel
                });
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
            product.ProductParameterProducts = product.ProductParameterProducts.Where(entry => entry.StartValue != null).ToList();
            if (ModelState.IsValid)
            {
                if (Seller.CurrentAuthorizedSellerId != null)
                {
                    product.SellerId = Seller.CurrentAuthorizedSellerId;
                }
                product.LastModified = DateTime.UtcNow;
                product.LastModifiedBy = User.Identity.Name;
                product.ProductParameterProducts.ForEach(entry => entry.ProductId = product.Id);
                db.ProductParameterProducts.RemoveRange(
                   db.ProductParameterProducts.Where(entry => entry.ProductId == product.Id));
                product.ProductParameterProducts = product.ProductParameterProducts.Distinct(new ProductParameterProductComparer()).ToList();
                db.ProductParameterProducts.AddRange(product.ProductParameterProducts);
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
                    product.AddedOn = DateTime.UtcNow;
                    db.Products.Add(product);
                }
                db.SaveChanges();
                TempData["SuccessMessage"] = "Товар збережено";
                return RedirectToAction("CreateOrUpdate", new { id = product.Id });
            }
            //todo: add check for seller role
            var ownerId = User.Identity.GetUserId();
            var seller = db.Sellers.FirstOrDefault(entry => ownerId == entry.Owner.Id) ?? db.Sellers.FirstOrDefault(entry => entry.Id == product.SellerId);
            var categories = db.Categories.Where(
                entry =>
                    entry.SellerCategories.Where(sc => !sc.IsDefault)
                        .Select(sc => sc.SellerId)
                        .Contains(Seller.CurrentAuthorizedSellerId)).ToList();
            categories =
                categories.Union(
                    db.Categories.Where(entry => entry.IsSellerCategory && entry.SellerId == product.SellerId)).ToList().SortByHierarchy().ToList();
            ViewBag.Categories = categories.Select(entry => new HierarchySelectItem()
            {
                Text = entry.Name,
                Value = entry.Id,
                Level = entry.HierarchicalLevel
            });
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
        public ActionResult RemoveFromExport(string id)
        {
            var productExport = db.ExportProducts.Where(entry => entry.ProductId == id);
            db.ExportProducts.RemoveRange(productExport);
            db.SaveChanges();
            return new HttpStatusCodeResult(200);
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

        public ActionResult BulkProductsAction(string[] productIds, ProductsBulkAction action, string category_Id, string export_Id, ProductFilterValues filters = null)
        {
            var productService = new ProductsService();
            List<string> products = null;
            List<string> existingExportProducts = null;
            switch (action)
            {
                case ProductsBulkAction.SetCategory:
                    db.Products.Where(entry => productIds.Contains(entry.Id))
                        .ForEach(entry =>
                        {
                            entry.CategoryId = category_Id;
                            db.Entry(entry).State = EntityState.Modified;
                        });
                    break;
                case ProductsBulkAction.DeleteSelected:
                    foreach (var productId in productIds)
                    {
                        productService.Delete(productId);
                    }
                    break;
                case ProductsBulkAction.DeleteAll:
                    products = GetFilteredProducts(filters).Select(entry => entry.Id).ToList();
                    foreach (var productId in products)
                    {
                        productService.Delete(productId);
                    }
                    break;
                case ProductsBulkAction.ExportSelected:
                    existingExportProducts = db.ExportProducts
                        .Where(entry => productIds.Contains(entry.ProductId) && entry.ExportId == export_Id)
                        .Select(entry => entry.ProductId).ToList();
                    productIds = productIds.Except(existingExportProducts).ToArray();
                    foreach (var productId in productIds)
                    {
                        var exportProduct = new ExportProduct()
                        {
                            ProductId = productId,
                            ExportId = export_Id
                        };
                        db.ExportProducts.Add(exportProduct);
                    }
                    break;
                case ProductsBulkAction.ExportAll:
                    products = GetFilteredProducts(filters).Select(entry => entry.Id).ToList();
                    existingExportProducts = db.ExportProducts
                        .Where(entry => products.Contains(entry.ProductId) && entry.ExportId == export_Id)
                        .Select(entry => entry.ProductId).ToList();
                    productIds = products.Except(existingExportProducts).ToArray();
                    var exportProducts = productIds.Select(entry => new ExportProduct
                    {
                        ProductId = entry,
                        ExportId = export_Id
                    });
                    db.ExportProducts.AddRange(exportProducts);
                    break;
                case ProductsBulkAction.DeleteFromExport:
                    var productExports = db.ExportProducts.Where(entry => productIds.Contains(entry.ProductId));
                    db.ExportProducts.RemoveRange(productExports);
                    break;
                case ProductsBulkAction.DeleteFromExportAll:
                    products = GetFilteredProducts(filters).Select(entry => entry.Id).ToList();
                    var productExportAlls = db.ExportProducts.Where(entry => products.Contains(entry.ProductId));
                    db.ExportProducts.RemoveRange(productExportAlls);
                    break;
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Товари успішно оброблено";
            return new HttpStatusCodeResult(200);
        }

        private IEnumerable<Product> GetFilteredProducts(ProductFilterValues filters)
        {
            IQueryable<Product> products =
                    db.Products
                        .Include(entry => entry.Images)
                        .Include(entry => entry.ExportProducts.Select(ep=>ep.Export))
                        .Include(entry => entry.ProductParameterProducts)
                        .AsQueryable();
            if (!string.IsNullOrEmpty(filters.CategoryId))
            {
                var categoryIds = new List<string>();
                var category = db.Categories.Include(entry => entry.MappedCategories)
                    .FirstOrDefault(entry => entry.Id == filters.CategoryId);
                var children = category.GetAllChildrenRecursively().ToList();
                categoryIds.Add(category.Id);
                categoryIds.AddRange(children.Select(cat => cat.Id));
                categoryIds.AddRange(children.SelectMany(entry => entry.MappedCategories).Select(entry => entry.Id));
                categoryIds.AddRange(category.MappedCategories.Select(entry => entry.Id));
                products = products.Where(entry => categoryIds.Contains(entry.CategoryId));
            }
            if (!string.IsNullOrEmpty(filters.SellerId))
            {
                products = products.Where(entry => entry.SellerId == filters.SellerId);
            }
            if (!string.IsNullOrEmpty(filters.ExportId))
            {
                products = products.Where(entry => entry.ExportProducts.Any(ep=>ep.ExportId == filters.ExportId));
            }
            if (Seller.CurrentAuthorizedSellerId != null)
            {
                products = products.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId);
            }
            if (filters.HasImage)
            {
                products = products.Where(entry => !entry.Images.Any());
            }
            if (filters.IsActive)
            {
                products = products.Where(entry => entry.IsActive);
            }
            if (filters.HasParameters.HasValue)
            {
                if (filters.HasParameters.Value)
                {
                    products = products.Where(entry => entry.ProductParameterProducts.Any());
                }
                else
                {
                    products = products.Where(entry => !entry.ProductParameterProducts.Any());
                }
            }
            if (filters.HasVendor.HasValue)
            {
                if (filters.HasVendor.Value)
                {
                    products = products.Where(entry => entry.Vendor != null);
                }
                else
                {
                    products = products.Where(entry => entry.Vendor == null);
                }
            }
            if (filters.IsAvailable.HasValue)
            {
                if (filters.IsAvailable.Value)
                {
                    products = products.Where(entry =>
                        entry.AvailabilityState != ProductAvailabilityState.NotInStock ||
                        entry.AvailableAmount > 0);
                }
                else
                {
                    products = products.Where(entry => entry.AvailabilityState == ProductAvailabilityState.NotInStock);
                }
            }
            if (filters.HasOriginCountry.HasValue)
            {
                if (filters.HasOriginCountry.Value)
                {
                    products = products.Where(entry => entry.OriginCountry != null);
                }
                else
                {
                    products = products.Where(entry => entry.OriginCountry == null);
                }
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
            return products;
        }
    }
}
