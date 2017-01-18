using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class BannersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Admin/Banners/
        public ActionResult Index()
        {
            return View(db.Banners.OrderBy(entry=>entry.Order).ToList());
        }

        [HttpPost]
        public ActionResult Index(List<string> sortedBanners)
        {
            var banners = db.Banners.ToList();
            for (var i = 0; i < sortedBanners.Count; i++)
            {
                var banner = banners.FirstOrDefault(entry => entry.Id == sortedBanners[i]);
                banner.Order = i;
                db.Entry(banner).State = EntityState.Modified;
            }
            db.SaveChanges();
            return Content(string.Empty);
        }

        public ActionResult CreateOrUpdate(string id = null)
        {
            var banner = db.Banners.Find(id) ?? new Banner() { Id = Guid.NewGuid().ToString() };
            return View(banner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrUpdate(Banner banner)
        {
            ModelState.Remove("ImageUrl");
            if (ModelState.IsValid)
            {
                var existingBanner = db.Banners.AsNoTracking().FirstOrDefault(entry => entry.Id == banner.Id);
                if (existingBanner != null)
                {
                    db.Entry(banner).State = EntityState.Modified;
                }
                else
                {
                    db.Banners.Add(banner);
                }
                var logo = Request.Files[0];
                if (logo != null && logo.ContentLength != 0)
                {
                    var fileName = Path.GetFileName(logo.FileName);
                    var dotIndex = fileName.IndexOf('.');
                    var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
                    var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                    var pathString = Path.Combine(originalDirectory, "Images", banner.BannerType.ToString());
                    banner.ImageUrl = banner.Id + fileExt;
                    logo.SaveAs(Path.Combine(pathString, banner.ImageUrl));
                }
                else
                {
                    if (existingBanner != null)
                    {
                        banner.ImageUrl = existingBanner.ImageUrl;
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(banner);
        }
        public ActionResult Delete(string id)
        {
            var banner = db.Banners.Find(id);
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var pathString = Path.Combine(originalDirectory, "Images", banner.BannerType.ToString(), banner.ImageUrl);
            var file = new FileInfo(pathString);
            file.Delete();
            db.Banners.Remove(banner);
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
