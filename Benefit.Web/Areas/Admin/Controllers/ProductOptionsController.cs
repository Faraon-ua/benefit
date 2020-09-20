using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Models;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class ProductOptionsController : AdminController
    {
        // GET: /Admin/ProductParameters/
        public ActionResult Index(string categoryId = null, string sellerId = null, string productId = null)
        {
            using (var db = new ApplicationDbContext())
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
                        db.Products
                            .Include(entry => entry.ProductOptions)
                            .Include(entry => entry.ProductOptions.Select(cd => cd.BindedProductOptions))
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
                    productOptions = db.ProductOptions
                        .Include(entry => entry.ChildProductOptions)
                        .Include(entry => entry.BindedProductOptions)
                        .Where(
                            entry =>
                                entry.CategoryId == categoryId && entry.SellerId == sellerId &&
                                entry.ParentProductOptionId == null).ToList();
                    productOptions.ForEach(entry => entry.Editable = true);
                }
                return View(new ProductOptionsViewModel
                {
                    Product = product,
                    ProductId = productId,
                    CategoryId = categoryId,
                    CategoryName = category == null ? null : category.Name,
                    SellerId = sellerId,
                    Sellers = db.Sellers.Select(entry => new SelectListItem() { Text = entry.Name, Value = entry.Id }).ToList(),
                    ProductOptions = productOptions
                });
            }
        }

        public ActionResult ProductOptionGroup(string id = null, string categoryId = null, string sellerId = null, string productId = null)
        {
            using (var db = new ApplicationDbContext())
            {
                var productParameter = db.ProductOptions.Find(id) ?? new ProductOption() { CategoryId = categoryId, SellerId = sellerId, ProductId = productId };
                return PartialView("_ProductOptionGroup", productParameter);
            }
        }

        public ActionResult ProductOptionValue(string parentId, string categoryId = null, string sellerId = null, string productId = null)
        {
            using (var db = new ApplicationDbContext())
            {
                var productParameter = new ProductOption() { ParentProductOptionId = parentId, CategoryId = categoryId, SellerId = sellerId, ProductId = productId, EditableAmount = true };
                return PartialView("_ProductOptionValue", productParameter);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrUpdate(ProductOption productparameter, HttpPostedFileBase image)
        {
            using (var db = new ApplicationDbContext())
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

                    if (image != null && image.ContentLength != 0)
                    {
                        var fileName = Path.GetFileName(image.FileName);
                        var dotIndex = fileName.IndexOf('.');
                        var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
                        var dir =
                            Server.MapPath("~/Images/ProductGallery/" + productparameter.ProductId + "/");
                        var path = Path.Combine(dir, productparameter.Id + fileExt);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        productparameter.Image = productparameter.Id + fileExt;
                        image.SaveAs(path);
                        var imageService = new ImagesService();
                        imageService.ResizeToSiteRatio(path, ImageType.ProductGallery);
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index", new { categoryId = productparameter.CategoryId, sellerId = productparameter.SellerId, productId = productparameter.ProductId });
                }
                return View(productparameter);
            }
        }

        [HttpPost]
        public ActionResult Index(List<string> sortedOptions)
        {
            using (var db = new ApplicationDbContext())
            {
                var options = db.ProductOptions.Where(entry => sortedOptions.Contains(entry.Id)).ToList();
                for (var i = 0; i < sortedOptions.Count; i++)
                {
                    var option = options.FirstOrDefault(entry => entry.Id == sortedOptions[i]);
                    option.Order = i;
                    db.Entry(option).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Json("Сортування опцій збережено");
            }
        }

        public ActionResult Delete(string id, string categoryId = null, string sellerId = null, string productId = null)
        {
            using (var db = new ApplicationDbContext())
            {
                var productOption = db.ProductOptions.Include("ChildProductOptions").FirstOrDefault(entry => entry.Id == id);
                db.ProductOptions.RemoveRange(productOption.ChildProductOptions);
                db.ProductOptions.Remove(productOption);
                foreach (var option in db.ProductOptions.Where(entry => entry.BindedProductOptionId == id))
                {
                    option.BindedProductOptionId = null;
                }

                var imagePath =
                    Server.MapPath("~/Images/ProductGallery/" + productOption.ProductId + "/" + productOption.Image);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                db.SaveChanges();
                return RedirectToAction("Index", new { categoryId, sellerId, productId });
            }
        }
        public ActionResult Connect(string optionId, string connectToOptionId)
        {
            using (var db = new ApplicationDbContext())
            {
                var option = db.ProductOptions.Find(optionId);
                if (option != null)
                {
                    option.BindedProductOptionId = string.IsNullOrEmpty(connectToOptionId) ? null : connectToOptionId;
                }
                db.SaveChanges();
                return new HttpStatusCodeResult(200);
            }
        }
    }
}
