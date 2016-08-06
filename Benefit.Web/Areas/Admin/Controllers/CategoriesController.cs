/*
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private LocalizationService LocalizationService = new LocalizationService();

        public ActionResult CategoriesBox(string parentCategoryId = null)
        {
            var cats = db.Categories.Where(entry => entry.ParentCategoryId == parentCategoryId);
            return PartialView("_CategoriesList", cats);
        }

        // GET: /Admin/Categories/
        public ActionResult Index()
        {
            var categories = db.Categories.Where(entry => entry.ParentCategoryId == null).OrderBy(entry => entry.Order);
            return View(categories.ToList());
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
        public ActionResult Create()
        {
            ViewBag.ParentCategoryId = new SelectList(db.Categories, "Id", "Name");
            var category = new Category() { Id = Guid.NewGuid().ToString() };
            category.Localizations = LocalizationService.Get(category, new[] {"Name", "Description"});

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category, HttpPostedFileBase categoryImage)
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
                category.IsVerified = true;
                category.LastModified = DateTime.UtcNow;
                category.LastModifiedBy = User.Identity.Name;
                db.Categories.Add(category);
                LocalizationService.Save(category.Localizations);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ParentCategoryId = new SelectList(db.Categories, "Id", "Name", category.ParentCategoryId);
            return View(category);
        }

        // GET: /Admin/Categories/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.ParentCategoryId = new SelectList(db.Categories, "Id", "Name", category.ParentCategoryId);
            return View(category);
        }

        // POST: /Admin/Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,ImageUrl,ParentCategoryId")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ParentCategoryId = new SelectList(db.Categories, "Id", "Name", category.ParentCategoryId);
            return View(category);
        }

        // GET: /Admin/Categories/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: /Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Index");
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
*/
