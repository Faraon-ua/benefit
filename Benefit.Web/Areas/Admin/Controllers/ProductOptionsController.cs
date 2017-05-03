using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Models;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class ProductOptionsController : AdminController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Admin/ProductParameters/
        public ActionResult Index(string categoryId = null, string sellerId = null, string productId = null)
        {
            if (sellerId == null && User.IsInRole(DomainConstants.SellerRoleName))
            {
                sellerId = Seller.CurrentAuthorizedSellerId;
            }
            List<ProductOption> productOptions;
            Product product = null;
            var category = db.Categories.FirstOrDefault(entry => entry.Id == categoryId);
            if (!string.IsNullOrEmpty(productId))
            {
                product =
                    db.Products.Include(entry => entry.ProductOptions)
                        .Include(entry => entry.Category)
                        .FirstOrDefault(entry => entry.Id == productId);
                productOptions = product.ProductOptions.Where(entry => entry.ParentProductOptionId == null).ToList();
                productOptions.ForEach(entry => entry.Editable = true);
                var categoryProductOptions = db.ProductOptions.Include(entry => entry.ChildProductOptions).Where(
                    entry =>
                        entry.CategoryId == product.CategoryId && entry.SellerId == sellerId &&
                        entry.ParentProductOptionId == null).ToList();
                productOptions.InsertRange(0, categoryProductOptions);
            }
            else
            {
                productOptions = db.ProductOptions.Include(entry=>entry.ChildProductOptions).Where(
                entry =>
                    entry.CategoryId == categoryId && entry.SellerId == sellerId && entry.ParentProductOptionId == null).ToList();
                productOptions.ForEach(entry => entry.Editable = true);
            }
            return View(new ProductOptionsViewModel
            {
                Product = product,
                ProductId =  productId,
                CategoryId = categoryId,
                CategoryName = category == null ? null : category.Name,
                SellerId = sellerId,
                Sellers = db.Sellers.Select(entry => new SelectListItem() { Text = entry.Name, Value = entry.Id }).ToList(),
                ProductOptions = productOptions
            });
        }

        public ActionResult ProductOptionGroup(string id = null, string categoryId = null, string sellerId = null, string productId = null)
        {
            var productParameter = db.ProductOptions.Find(id) ?? new ProductOption() { CategoryId = categoryId, SellerId = sellerId, ProductId = productId };
            return PartialView("_ProductOptionGroup", productParameter);
        }

        public ActionResult ProductOptionValue(string parentId, string categoryId = null, string sellerId = null, string productId = null)
        {
            var productParameter = new ProductOption() { ParentProductOptionId = parentId, CategoryId = categoryId, SellerId = sellerId, ProductId = productId, EditableAmount =  true };
            return PartialView("_ProductOptionValue", productParameter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrUpdate(ProductOption productparameter)
        {
            if (ModelState.IsValid)
            {
                if (!db.ProductOptions.Any(entry => entry.Id == productparameter.Id))
                {
                    productparameter.Id = Guid.NewGuid().ToString();
                    db.ProductOptions.Add(productparameter);
                }
                else
                {
                    db.Entry(productparameter).State = EntityState.Modified;
                }
                db.SaveChanges();
                return RedirectToAction("Index", new { categoryId = productparameter.CategoryId, sellerId = productparameter.SellerId, productId = productparameter.ProductId });
            }
            return View(productparameter);
        }

        public ActionResult Delete(string id, string categoryId = null, string sellerId = null, string productId = null)
        {
            var productOption = db.ProductOptions.Include("ChildProductOptions").FirstOrDefault(entry => entry.Id == id);
            db.ProductOptions.RemoveRange(productOption.ChildProductOptions);
            db.ProductOptions.Remove(productOption);
            db.SaveChanges();
            return RedirectToAction("Index", new { categoryId, sellerId, productId });
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
