using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class ImagesController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        ImagesService ImagesService = new ImagesService();

        public ActionResult SellerCatalog()
        {
            var images = db.Images.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId && entry.ImageType == ImageType.SellerCatalog).OrderBy(entry => entry.Order).ToList();
            return View(images);
        }

        public ActionResult DeleteSellerCatalogImage(string id)
        {
            ImagesService.Delete(id, Seller.CurrentAuthorizedSellerId, ImageType.SellerCatalog);
            return RedirectToAction("SellerCatalog");
        }

        public ActionResult AddSellerCatalogImage()
        {
            if (Request.Files.Count == 0)
            {
                TempData["ErrorMessage"] = "Файл не вибрано";
                return RedirectToAction("SellerCatalog");
            }
            var sellerLogo = Request.Files[0];
            var fileName = Path.GetFileName(sellerLogo.FileName);
            var dotIndex = fileName.IndexOf('.');
            var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var pathString = Path.Combine(originalDirectory, "Images", ImageType.SellerCatalog.ToString(), Seller.CurrentAuthorizedSellerId);
            var img = new Image()
            {
                Id = Guid.NewGuid().ToString(),
                ImageType = ImageType.SellerCatalog,
                SellerId = Seller.CurrentAuthorizedSellerId
            };
            img.ImageUrl = img.Id + fileExt;
            
            db.Images.Add(img);
            sellerLogo.SaveAs(Path.Combine(pathString, img.ImageUrl));
            var imagesService = new ImagesService();
            imagesService.ResizeToSiteRatio(Path.Combine(pathString, img.ImageUrl), ImageType.SellerLogo);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Файл каталогу збережено";
            return RedirectToAction("SellerCatalog");
        }
    }
}