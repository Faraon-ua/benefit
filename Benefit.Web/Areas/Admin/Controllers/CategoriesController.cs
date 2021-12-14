using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Services.Domain;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Helpers;
using Benefit.Web.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Domain.Models.ModelExtensions;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class CategoriesController : AdminController
    {
        private LocalizationService LocalizationService = new LocalizationService();

        [Authorize(Roles = "Admin, Seller")]
        public ActionResult CategoriesList(string parentCategoryId = null)
        {
            using (var db = new ApplicationDbContext())
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
                var cats = db.Categories.Where(entry => entry.ParentCategoryId == parentCategoryId && !entry.IsSellerCategory).OrderBy(entry => entry.Order).ToList();
                var viewModel = new KeyValuePair<string, IEnumerable<Category>>(parentCategoryId, cats);
                return PartialView("_CategoriesList", viewModel);
            }
        }
        // GET: /Admin/Categories/
        [Authorize(Roles = "Admin, Seller")]
        public ActionResult Index(string search = null)
        {
            using (var db = new ApplicationDbContext())
            {
                var model = new List<Category>();
                if (!string.IsNullOrEmpty(search))
                {
                    model = db.Categories.Where(entry => entry.Name.ToLower().Contains(search.ToLower()) || entry.Id == search).ToList();
                }
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult Index(List<string> sortedCategories)
        {
            using (var db = new ApplicationDbContext())
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
        }

        public ActionResult DeactivateEmptyCategories()
        {
            using (var db = new ApplicationDbContext())
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
        }

        // GET: /Admin/Categories/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult CreateOrUpdate(string id = null, string parentCategoryId = null)
        {
            using (var db = new ApplicationDbContext())
            {
                var category = db.Categories
                               .Include(entry => entry.ExportCategories.Select(ec => ec.Export))
                               .Include(entry => entry.SellerCategories.Select(sc => sc.Category))
                               .Include(entry => entry.SellerCategories.Select(sc => sc.Seller))
                               .FirstOrDefault(entry => entry.Id == id) ??
                           new Category()
                           {
                               Id = Guid.NewGuid().ToString(),
                               ParentCategoryId = parentCategoryId
                           };
                var categories = db.Categories.Where(entry => !entry.IsSellerCategory).ToList().SortByHierarchy().ToList().Select(entry => new HierarchySelectItem()
                {
                    Text = entry.Name,
                    Value = entry.Id,
                    Level = entry.HierarchicalLevel
                }).ToList();
                categories.Insert(0, new HierarchySelectItem() { Text = "Не обрано", Value = string.Empty, Level = 0 });
                ViewBag.Categories = categories;
                ViewBag.Exports = db.ExportImports.Where(entry => entry.SyncType == SyncType.YmlExport || entry.SyncType == SyncType.YmlExportEpicentr).ToList();
                category.Localizations = LocalizationService.Get(category,
                    entry => entry.Name,
                    entry => entry.Description,
                    entry => entry.MetaDescription,
                    entry => entry.Title);
                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [ValidateInput(false)]
        public ActionResult CreateOrUpdate(Category category, HttpPostedFileBase categoryImage, HttpPostedFileBase categoryBannerImage)
        {
            using (var db = new ApplicationDbContext())
            {
                for (var i = 0; i < category.ExportCategories.Count; i++)
                {
                    if (category.ExportCategories.ElementAt(i).Name == null)
                    {
                        foreach (var key in ModelState.Keys.Where(entry => entry.Contains(string.Format("ExportCategories[{0}]", i))))
                        {
                            ModelState[key].Errors.Clear();
                        }
                    }
                }
                for (var i = 0; i < category.Localizations.Count; i++)
                {
                    if (category.Localizations[i].ResourceValue == null)
                    {
                        foreach (var key in ModelState.Keys.Where(entry => entry.Contains(string.Format("Localizations[{0}]", i))))
                        {
                            ModelState[key].Errors.Clear();
                        }
                    }
                }
                if (db.Categories.Any(entry => entry.UrlName == category.UrlName && entry.Id != category.Id))
                {
                    ModelState.AddModelError("UrlName", "Категорія з таким Url вже існує");
                }
                if (ModelState.IsValid)
                {
                    var exportCategories = category.ExportCategories.Where(entry => entry != null).ToList();
                    category.LastModified = DateTime.UtcNow;
                    category.LastModifiedBy = User.Identity.Name;

                    if (db.Categories.Any(entry => entry.Id == category.Id))
                    {
                        db.Entry(category).State = EntityState.Modified;
                        var existingCategoryExports = db.ExportCategories.Where(entry => entry.CategoryId == category.Id).ToList();
                        db.ExportCategories.RemoveRange(existingCategoryExports);
                        db.SaveChanges();
                        db.ExportCategories.AddRange(exportCategories.Where(entry => entry.Name != null));
                        var existingCat = db.Categories.Find(category.Id);
                        var children = existingCat.GetAllChildrenRecursively().Distinct(new CategoryComparer()).ToList();
                        children.ForEach(entry =>
                                            {
                                                var local = db.Set<Category>()
                                                    .Local
                                                    .FirstOrDefault(f => f.Id == entry.Id);
                                                if (local != null)
                                                {
                                                    db.Entry(local).State = EntityState.Detached;
                                                }
                                                entry.ShowCartOnOrder = category.ShowCartOnOrder;
                                                db.Entry(entry).State = EntityState.Modified;
                                            });
                        var childernIds = children.Select(entry => entry.Id).ToList();
                        var mappedCats = db.Categories.Where(entry => childernIds.Contains(entry.MappedParentCategoryId) || entry.MappedParentCategoryId == existingCat.Id).ToList();
                        mappedCats.ForEach(entry =>
                        {
                            entry.ShowCartOnOrder = category.ShowCartOnOrder;
                            db.Entry(entry).State = EntityState.Modified;
                        });
                    }
                    else
                    {
                        category.Order = db.Categories.Max(entry => entry.Order) + 1;
                        db.Categories.Add(category);
                    }
                    if (categoryImage != null && categoryImage.ContentLength > 0)
                    {
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
                    if (categoryBannerImage != null && categoryBannerImage.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(categoryBannerImage.FileName);
                        var dotIndex = fileName.IndexOf('.');
                        var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
                        var path = Path.Combine(Server.MapPath("~/Images/CategoryBanner/"), category.Id + fileExt);
                        category.BannerImageUrl = category.Id + fileExt;
                        categoryBannerImage.SaveAs(path);
                    }
                    else
                    {
                        db.Entry(category).Property(entry => entry.BannerImageUrl).IsModified = false;
                    }
                    LocalizationService.Save(category.Localizations);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Категорію було збережено";
                    return RedirectToAction("CreateOrUpdate", new { id = category.Id });
                }

                var categories =
                    db.Categories.Where(entry => !entry.IsSellerCategory).ToList().SortByHierarchy().ToList().Select(
                        entry => new HierarchySelectItem()
                        {
                            Text = entry.Name,
                            Value = entry.Id,
                            Level = entry.HierarchicalLevel
                        }).ToList();
                categories.Insert(0, new HierarchySelectItem() { Text = "Не обрано", Value = string.Empty, Level = 0 });
                ViewBag.Categories = categories;
                ViewBag.Exports = db.ExportImports.Where(entry => entry.SyncType == SyncType.YmlExport || entry.SyncType == SyncType.YmlExport).ToList();
                return View(category);
            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var categoriesService = new CategoriesService();
            categoriesService.Delete(id);
            return Json(true);
        }

        private void SetMappedCategories(List<Category> categories, ApplicationDbContext db)
        {
            categories.ForEach(entry => entry.MappedParentCategory = db.Categories.Find(entry.MappedParentCategoryId));
            foreach (var cat in categories)
            {
                SetMappedCategories(cat.ChildCategories.ToList(), db);
            }
        }

        public ActionResult RemoveInactive(string sellerId)
        {
            using (var db = new ApplicationDbContext())
            {
                var categories = db.Categories.Where(entry => entry.SellerId == sellerId && !entry.IsActive).Select(entry => entry.Id).ToList();
                var categoriesService = new CategoriesService();
                for (var i = 0; i < categories.Count; i++)
                {
                    categoriesService.Delete(categories[i]);
                }
                return new HttpStatusCodeResult(200);
            }
        }

        public ActionResult Mapping(string selectedSellerId = null)
        {
            using (var db = new ApplicationDbContext())
            {
                var sellerId = selectedSellerId ?? Seller.CurrentAuthorizedSellerId;
                var categoriesToMap = db.Categories.AsNoTracking()
                    .Include(entry => entry.ChildCategories.Select(ch =>
                        ch.ChildCategories.Select(sub => sub.ChildCategories.Select(chsub => chsub.ChildCategories))))
                    .Where(entry => entry.SellerId == sellerId && entry.IsSellerCategory && entry.ParentCategoryId == null)
                    .ToList();
                SetMappedCategories(categoriesToMap, db);
                ViewBag.RootCategories = db.Categories.Where(entry => entry.IsActive && !entry.IsSellerCategory && entry.ParentCategoryId == null && entry.Order > 0).OrderBy(entry => entry.Order).ToList();
                var importTask = db.ExportImports.FirstOrDefault(entry => entry.SellerId == sellerId);
                if (importTask != null)
                {
                    importTask.HasNewContent = false;
                    db.SaveChanges();
                }
                return View(categoriesToMap);
            }
        }

        public ActionResult SearchCategories(string search)
        {
            using (var db = new ApplicationDbContext())
            {
                var categories = db.Categories.Include(entry => entry.ChildCategories).Where(entry => !entry.ChildCategories.Any() && entry.Name.ToLower().Contains(search.ToLower())).ToList();
                return PartialView("_MappingSearchCategories", categories);
            }
        }

        public ActionResult GetMappingCategories(string parentId)
        {
            using (var db = new ApplicationDbContext())
            {
                var cats =
                db.Categories.Where(
                    entry => entry.ParentCategoryId == parentId && entry.IsActive && !entry.IsSellerCategory).OrderBy(entry => entry.Order).ToList();
                var partialHtml = ControllerContext.RenderPartialToString("_MappingSiteCategoriesList", cats);
                return Json(new { html = partialHtml, selectable = !cats.Any() }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult MapCategories(string sellerCatId, string siteCatId)
        {
            using (var db = new ApplicationDbContext())
            {
                var sellerCat = db.Categories.FirstOrDefault(entry => entry.Id == sellerCatId);
                if (sellerCat == null)
                {
                    return HttpNotFound();
                }

                sellerCat.MappedParentCategoryId = siteCatId;
                db.Entry(sellerCat).State = EntityState.Modified;
                db.SaveChanges();
                return new HttpStatusCodeResult(200);
            }
        }
    }
}
