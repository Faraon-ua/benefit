using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Web.Areas.Admin.Controllers.Base;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class InfoPagesController : AdminController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private LocalizationService LocalizationService = new LocalizationService();

        // GET: /Admin/InfoPages/
        public ActionResult Index()
        {
            return View(db.InfoPages.OrderBy(entry => entry.Order).ToList());
        }

        // GET: /Admin/InfoPages/Create
        public ActionResult CreateOrUpdate(string id = null)
        {
            var infoPage = db.InfoPages.Find(id) ?? new InfoPage() { Id = Guid.NewGuid().ToString() };
            infoPage.Localizations = LocalizationService.Get(infoPage, new[] { "Name", "Content" });
            return View(infoPage);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrUpdate(InfoPage infopage)
        {
            if (ModelState.IsValid)
            {
                infopage.CreatedOn = DateTime.UtcNow;
                infopage.LastModified = DateTime.UtcNow;
                infopage.LastModifiedBy = User.Identity.Name;
                var logo = Request.Files[0];
                if (logo != null && logo.ContentLength != 0)
                {
                    var newsLogo = Request.Files[0];
                    var fileName = Path.GetFileName(newsLogo.FileName);
                    var dotIndex = fileName.IndexOf('.');
                    var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
                    var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                    var relativePathString = Path.Combine("Images", ImageType.NewsLogo.ToString(), infopage.Id + fileExt);
                    var pathString = Path.Combine(originalDirectory, relativePathString);
                    newsLogo.SaveAs(pathString);
                    infopage.ImageUrl = infopage.Id + fileExt;
                    var imagesService = new ImagesService();
                    imagesService.ResizeToSiteRatio(pathString, ImageType.NewsLogo);
                }
                if (db.InfoPages.Any(entry => entry.Id == infopage.Id))
                {
                    db.Entry(infopage).State = EntityState.Modified;
                }
                else
                {
                    db.InfoPages.Add(infopage);
                }
                LocalizationService.Save(infopage.Localizations);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Сторінку збережено";
                return RedirectToAction("CreateOrUpdate", new { id=infopage.Id });
            }

            return View(infopage);
        }

        public ActionResult Delete(string id)
        {
            var infopage = db.InfoPages.Find(id);
            if (infopage != null)
            {
                if (!string.IsNullOrEmpty(infopage.ImageUrl))
                {
                    var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                    var pathString = Path.Combine(originalDirectory, infopage.ImageUrl);
                    if (System.IO.File.Exists(pathString))
                        System.IO.File.Delete(pathString);
                }
                LocalizationService.Delete(id);
                db.InfoPages.Remove(infopage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return HttpNotFound();
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
