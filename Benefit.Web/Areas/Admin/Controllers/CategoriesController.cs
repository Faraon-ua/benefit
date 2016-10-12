using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Services.Domain;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class CategoriesController : AdminController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private LocalizationService LocalizationService = new LocalizationService();

        [Authorize(Roles = "Admin, Seller")]
        public ActionResult CategoriesList(string parentCategoryId = null)
        {
            if (parentCategoryId == null && User.IsInRole("Seller"))
            {
                var userId = User.Identity.GetUserId();
                var seller = db.Sellers.FirstOrDefault(entry => userId == entry.OwnerId);
                if (seller.SellerCategories.Any())
                {
                    parentCategoryId = seller.SellerCategories.First().CategoryId;
                }
            }
            var cats = db.Categories.Where(entry => entry.ParentCategoryId == parentCategoryId).OrderBy(entry => entry.Order);
            var viewModel = new KeyValuePair<string, IEnumerable<Category>>(parentCategoryId, cats);
            return PartialView("_CategoriesList", viewModel);
        }

        // GET: /Admin/Categories/
        [Authorize(Roles = "Admin, Seller")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(List<string> sortedCategories)
        {
            var cats = db.Categories.ToList();
            for (var i = 0; i < sortedCategories.Count; i++)
            {
                var cat = cats.FirstOrDefault(entry => entry.Id == sortedCategories[i]);
                cat.Order = i;
                db.Entry(cat).State = EntityState.Modified;
            }
            db.SaveChanges();
            return Json("Сортування категорій збережено");
        }

        // GET: /Admin/Categories/Create
        [Authorize(Roles = "Admin, Seller")]
        public ActionResult CreateOrUpdate(string id = null, string parentCategoryId = null)
        {
            var category = db.Categories.Find(id) ?? new Category() { Id = Guid.NewGuid().ToString(), ParentCategoryId = parentCategoryId, NavigationType = CategoryNavigationType.SellersAndProducts.ToString()};
            ViewBag.ParentCategoryId = new SelectList(db.Categories.Where(entry => entry.Id != category.Id), "Id", "ExpandedName", category.ParentCategoryId);
            category.Localizations = LocalizationService.Get(category, new[] { "Name", "Description" });
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Seller")]
        public ActionResult CreateOrUpdate(Category category, HttpPostedFileBase categoryImage)
        {
            if (categoryImage != null && categoryImage.ContentLength > 0)
            {
                //todo: add image resizing
                var fileName = Path.GetFileName(categoryImage.FileName);
                var dotIndex = fileName.IndexOf('.');
                var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
                var path = Path.Combine(Server.MapPath("~/Images/"), category.Id + fileExt);
                category.ImageUrl = category.Id + fileExt;
                categoryImage.SaveAs(path);
            }
            if (ModelState.IsValid)
            {
                category.LastModified = DateTime.UtcNow;
                category.LastModifiedBy = User.Identity.Name;
                if (db.Categories.Any(entry => entry.Id == category.Id))
                {
                    db.Entry(category).State = EntityState.Modified;
                }
                else
                {
                    category.Order = db.Categories.Max(entry => entry.Order) + 1;
                    db.Categories.Add(category);
                    LocalizationService.Save(category.Localizations);
                }
                db.SaveChanges();
                TempData["SuccessMessage"] = "Категорію було збережено";
                return RedirectToAction("Index");
            }

            ViewBag.ParentCategoryId = new SelectList(db.Categories.Where(entry => entry.Id != category.Id), "Id", "ExpandedName", category.ParentCategoryId);
            return View(category);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var categoriesService = new CategoriesService();
            categoriesService.Delete(id);
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
