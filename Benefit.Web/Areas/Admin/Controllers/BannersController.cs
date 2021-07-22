using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Web.Helpers;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class BannersController : Controller
    {
        // GET: /Admin/Banners/
        public ActionResult Index()
        {
            using (var db = new ApplicationDbContext())
            {
                var banners = db.Banners.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId).OrderBy(entry => entry.Order).ToList();
                return View(banners);
            }
        }

        [HttpPost]
        public ActionResult Index(List<string> sortedBanners)
        {
            using (var db = new ApplicationDbContext())
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
        }

        public ActionResult CreateOrUpdate(string id = null, int? order = null)
        {
            using (var db = new ApplicationDbContext())
            {
                var banner = db.Banners.Find(id) ?? new Banner() { Id = Guid.NewGuid().ToString(), Order = order ?? default(int), SellerId = Seller.CurrentAuthorizedSellerId };
                var bannerTypes = new Banner().BannerType.ToSelectList();
                if (Seller.CurrentAuthorizedSellerId != null)
                {
                    bannerTypes = new SelectList(bannerTypes.Items.OfType<SelectListItem>()
                        .Where(entry=>entry.Value == BannerType.PrimaryMainPage.ToString() || entry.Value == BannerType.SideTopMainPage.ToString()).ToList(),
                        "Value", "Text", banner.BannerType.ToString());
                }
                ViewBag.BannerType = bannerTypes;
                return View(banner);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreateOrUpdate(Banner banner)
        {
            using (var db = new ApplicationDbContext())
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
                        var imageService = new ImagesService();
                        var format = imageService.GetImageFormatByExtension(logo.FileName);
                        var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                        var pathString = Path.Combine(originalDirectory, "Images", banner.BannerType.ToString());
                        banner.ImageUrl = banner.Id + ".webp";
                        var path = Path.Combine(pathString, banner.ImageUrl);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                        logo.SaveAs(path);
                        SellerEcommerceTemplate? sellerTemplate = null;
                        if (Seller.CurrentAuthorizedSellerId != null)
                        {
                            var seller = db.Sellers.Find(Seller.CurrentAuthorizedSellerId);
                            sellerTemplate = seller.EcommerceTemplate;
                        }
                        imageService.ResizeToSiteRatio(path, ImageType.Banner, banner.BannerType, imageFormat: format, sellerTemplate);
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
                var bannerTypes = new Banner().BannerType.ToSelectList();
                if (Seller.CurrentAuthorizedSellerId != null)
                {
                    bannerTypes = new SelectList(bannerTypes.Take(2));
                }
                ViewBag.BannerType = bannerTypes;
                return View(banner);
            }
        }
        public ActionResult Delete(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var banner = db.Banners.Find(id);
                var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                var pathString = Path.Combine(originalDirectory, "Images", banner.BannerType.ToString(), banner.ImageUrl);
                var file = new FileInfo(pathString);
                if (file.Exists)
                {
                    file.Delete();
                }
                db.Banners.Remove(banner);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }
    }
}
