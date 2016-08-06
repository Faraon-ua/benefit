/*
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class InfoPagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private LocalizationService LocalizationService = new LocalizationService();

        // GET: /Admin/InfoPages/
        public ActionResult Index()
        {
            return View(db.InfoPages.OrderBy(entry=>entry.Order).ToList());
        }

        // GET: /Admin/InfoPages/Create
        public ActionResult Create()
        {
            var infoPage = new InfoPage() {Id = Guid.NewGuid().ToString()};
            infoPage.Localizations = LocalizationService.Get(infoPage, new[] { "Name", "Content" });
            return View(infoPage);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InfoPage infopage)
        {
            if (ModelState.IsValid)
            {
                infopage.LastModified = DateTime.UtcNow;
                infopage.LastModifiedBy = User.Identity.Name;
                db.InfoPages.Add(infopage);
                LocalizationService.Save(infopage.Localizations);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(infopage);
        }

        // GET: /Admin/InfoPages/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InfoPage infopage = db.InfoPages.Find(id);
            if (infopage == null)
            {
                return HttpNotFound();
            }
            return View(infopage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(InfoPage infopage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(infopage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(infopage);
        }

        public ActionResult Delete(string id)
        {
            var infopage = db.InfoPages.Find(id);
//            db.InfoPages.Remove(infopage);
            return RedirectToAction("Index");
        }

       /* // POST: /Admin/InfoPages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            InfoPage infopage = db.InfoPages.Find(id);
            db.InfoPages.Remove(infopage);
            db.SaveChanges();
            return RedirectToAction("Index");
        }#1#

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
