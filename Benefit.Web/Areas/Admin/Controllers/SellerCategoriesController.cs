using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Seller, SellerOperator")]
    public class SellerCategoriesController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new ApplicationDbContext())
            {
                var sellerId = Seller.CurrentAuthorizedSellerId;
                var sellerCategories = db.SellerCategories.Include(entry => entry.Category).Where(entry => entry.SellerId == sellerId).OrderBy(entry => entry.Order).ToList();
                return View(sellerCategories);
            }
        }

        public ActionResult CreateOrUpdate(string id = null)
        {
            var sellerId = Seller.CurrentAuthorizedSellerId;
            using (var db = new ApplicationDbContext())
            {
                var category = db.SellerCategories
                    .Include(entry=>entry.Category)
                    .FirstOrDefault(entry => entry.CategoryId == id && entry.SellerId == sellerId);
                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreateOrUpdate(SellerCategory category, HttpPostedFileBase categoryImage)
        {
            var sellerId = Seller.CurrentAuthorizedSellerId;
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    var existingSC = db.SellerCategories.FirstOrDefault(entry => entry.CategoryId == category.CategoryId && entry.SellerId == sellerId);
                    db.Entry(existingSC).State = EntityState.Modified;
                    existingSC.CustomName = category.CustomName;
                    existingSC.Order = category.Order;
                    if (categoryImage != null && categoryImage.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(categoryImage.FileName);
                        var dotIndex = fileName.LastIndexOf('.');
                        var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
                        var path = Path.Combine(Server.MapPath("~/Images/CategoryLogo/"), category.CategoryId + sellerId + fileExt);
                        existingSC.CustomImageUrl = category.Category + category.SellerId + fileExt;
                        categoryImage.SaveAs(path);
                        var imageService = new ImagesService();
                        imageService.ResizeToSiteRatio(path, ImageType.CategoryLogo);
                    }
                    else
                    {
                        db.Entry(existingSC).Property(entry => entry.CustomImageUrl).IsModified = false;
                    }
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Категорію було збережено";
                    return RedirectToAction("CreateOrUpdate", new { id = category.CategoryId });
                }
                return View(category);
            }
        }

        public ActionResult SaveCategoriesOrder(List<string> sortedCats)
        {
            var sellerId = Seller.CurrentAuthorizedSellerId;
            using (var db = new ApplicationDbContext())
            {
                var categories = db.SellerCategories.Where(entry => sortedCats.Contains(entry.CategoryId) && entry.SellerId == sellerId).ToList();
                for (var i = 0; i < sortedCats.Count; i++)
                {
                    var img = categories.FirstOrDefault(entry => entry.CategoryId == sortedCats[i]);
                    img.Order = i;
                    db.Entry(img).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
            return Json(new { message = "Сортування категорій збережено" });
        }
    }
}