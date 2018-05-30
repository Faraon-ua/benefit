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
using Benefit.Services.Domain;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Helpers;
using Microsoft.AspNet.Identity;
using WebGrease.Css.Extensions;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class CategoriesController : AdminController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private LocalizationService LocalizationService = new LocalizationService();

        [Authorize(Roles = "Admin, Seller")]
        public ActionResult CategoriesList(string parentCategoryId = null)
        {
            if (parentCategoryId == null && User.IsInRole(DomainConstants.SellerRoleName) && !User.IsInRole(DomainConstants.AdminRoleName))
            {
                var userId = User.Identity.GetUserId();
                var seller = db.Sellers.FirstOrDefault(entry => userId == entry.OwnerId);
                if (seller.SellerCategories.Any())
                {
                    parentCategoryId = seller.SellerCategories.First().CategoryId;
                }
            }
            var cats = db.Categories.Where(entry => entry.ParentCategoryId == parentCategoryId && !entry.IsSellerCategory).OrderBy(entry => entry.Order);
            var viewModel = new KeyValuePair<string, IEnumerable<Category>>(parentCategoryId, cats);
            return PartialView("_CategoriesList", viewModel);
        }

        // GET: /Admin/Categories/
        [Authorize(Roles = "Admin, Seller")]
        public ActionResult Index(string search = null)
        {
            var model = new List<Category>();
            if (!string.IsNullOrEmpty(search))
            {
                model = db.Categories.Where(entry => entry.Name.ToLower().Contains(search.ToLower())).ToList();
            }
            return View(model);
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
            db.Configuration.ValidateOnSaveEnabled = false;
            db.SaveChanges();
            return Json("Сортування категорій збережено");
        }

        public ActionResult DeactivateEmptyCategories()
        {
            var cats = new List<Category>();
            do
            {
                cats = db.Categories
                    .Include(entry => entry.Products.Select(pr => pr.Seller))
                    .Include(entry => entry.ChildCategories)
                    .Where(entry =>
                        entry.IsActive && !entry.IsSellerCategory &&
                        !(entry.ChildCategories.Any(cc => cc.IsActive) ||
                          entry.Products.Any(pr => pr.IsActive && pr.Seller.IsActive))).ToList();
                cats.ForEach(entry => entry.IsActive = false);
                db.SaveChanges();
            } while (cats.Any());

            TempData["SuccessMessage"] = "Пусті категорі було деактивовано";
            return RedirectToAction("Index");
        }

        // GET: /Admin/Categories/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult CreateOrUpdate(string id = null, string parentCategoryId = null)
        {
            var category = db.Categories.Include(entry => entry.SellerCategories.Select(sc => sc.Category)).FirstOrDefault(entry => entry.Id == id) ??
                           new Category()
                           {
                               Id = Guid.NewGuid().ToString(),
                               ParentCategoryId = parentCategoryId,
                               NavigationType = CategoryNavigationType.SellersAndProducts.ToString()
                           };
            ViewBag.ParentCategoryId = new SelectList(db.Categories.Where(entry => entry.Id != category.Id && entry.SellerId == null), "Id",
                "ExpandedName", category.ParentCategoryId);
            category.Localizations = LocalizationService.Get(category, new[] { "Name", "Description" });
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult CreateOrUpdate(Category category, HttpPostedFileBase categoryImage)
        {
            if (db.Categories.Any(entry => entry.UrlName == category.UrlName && entry.Id != category.Id))
            {
                ModelState.AddModelError("UrlName", "Категорія з таким Url вже існує");
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
                if (categoryImage != null && categoryImage.ContentLength > 0)
                {
                    //todo: add image resizing
                    var fileName = Path.GetFileName(categoryImage.FileName);
                    var dotIndex = fileName.IndexOf('.');
                    var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
                    var path = Path.Combine(Server.MapPath("~/Images/CategoryLogo/"), category.Id + fileExt);
                    category.ImageUrl = category.Id + fileExt;
                    categoryImage.SaveAs(path);
                    var imageService = new ImagesService();
                    imageService.ResizeToSiteRatio(path, ImageType.CategoryLogo);
                }
                else
                {
                    db.Entry(category).Property(entry => entry.ImageUrl).IsModified = false;
                }
                db.SaveChanges();
                TempData["SuccessMessage"] = "Категорію було збережено";
                return RedirectToAction("CreateOrUpdate", new { id = category.Id });
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

        private void SetMappedCategories(List<Category> categories)
        {
            categories.ForEach(entry => entry.MappedParentCategory = db.Categories.Find(entry.MappedParentCategoryId));
            foreach (var cat in categories)
            {
                SetMappedCategories(cat.ChildCategories.ToList());
            }
        }

        public ActionResult Mapping()
        {
            var sellerId = Seller.CurrentAuthorizedSellerId;
            var categoriesToMap = db.Categories.Include(entry => entry.ChildCategories.Select(ch => ch.ChildCategories)).Where(entry => entry.SellerId == sellerId && entry.IsSellerCategory && entry.ParentCategoryId == null).ToList();
            SetMappedCategories(categoriesToMap);
            ViewBag.RootCategories = db.Categories.Where(entry => entry.IsActive && !entry.IsSellerCategory && entry.ParentCategoryId == null && entry.Order > 0).OrderBy(entry => entry.Order).ToList();
            return View(categoriesToMap);
        }

        public ActionResult SearchCategories(string search)
        {
            var categories = db.Categories.Include(entry => entry.ChildCategories).Where(entry => !entry.ChildCategories.Any() && entry.Name.ToLower().Contains(search.ToLower())).ToList();
            return PartialView("_MappingSearchCategories", categories);
        }

        public ActionResult GetMappingCategories(string parentId)
        {
            var cats =
                db.Categories.Where(
                    entry => entry.ParentCategoryId == parentId && entry.IsActive && !entry.IsSellerCategory).OrderBy(entry => entry.Order).ToList();
            var partialHtml = ControllerContext.RenderPartialToString("_MappingSiteCategoriesList", cats);
            return Json(new { html = partialHtml, selectable = !cats.Any() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MapCategories(string sellerCatId, string siteCatId)
        {
            var sellerCat = db.Categories.FirstOrDefault(entry => entry.Id == sellerCatId);
            if (sellerCat == null) return HttpNotFound();
            sellerCat.MappedParentCategoryId = siteCatId;
            db.Entry(sellerCat).State = EntityState.Modified;
            db.SaveChanges();
            return new HttpStatusCodeResult(200);
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
