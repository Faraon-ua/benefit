using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.JSON;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;
using Benefit.Services;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;
using Benefit.Web.Models;

namespace Benefit.Web.Controllers
{
    public class HomeController : BaseController
    {
        [FetchSeller]
        [FetchCategories]
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Any, Duration = CacheConstants.OutputCacheLength)]
        public async Task<ActionResult> Index()
        {
            //handle seller subdomain
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                using (var db = new ApplicationDbContext())
                {
                    seller.FeaturedProducts = db.Products
                        .Include(entry => entry.Images)
                        .Include(entry => entry.Seller)
                        .Where(entry =>
                            entry.IsActive && entry.SellerId == seller.Id && entry.IsFeatured).ToList();
                    seller.PromotionProducts = db.Products
                        .Include(entry => entry.Images)
                        .Include(entry => entry.Seller)
                        .Where(entry =>
                            entry.IsActive && entry.SellerId == seller.Id && entry.OldPrice != null).ToList();
                }

                if (seller.HasEcommerce)
                {
                    return View("~/views/sellerarea/home.cshtml", seller);
                }
                return View("~/views/sellerareabc/home.cshtml", seller);
            }
            var mainPageViewModel = new MainPageViewModel();
            using (var db = new ApplicationDbContext())
            {
                mainPageViewModel.FeaturedProducts = db.Products
                    .Include(entry => entry.Reviews)
                    .Include(entry => entry.Images)
                    .Include(entry => entry.Category)
                    .Include(entry => entry.Seller)
                    .Where(entry => entry.IsFeatured).OrderBy(entry => entry.Order).ToList()
                    .Select(entry => new ProductPartialViewModel()
                    {
                        Product = entry
                    }).ToList();
                mainPageViewModel.NewProducts = db.Products
                    .Include(entry => entry.Reviews)
                    .Include(entry => entry.Images)
                    .Include(entry => entry.Category)
                    .Include(entry => entry.Seller)
                    .Where(entry => entry.IsNewProduct).OrderBy(entry => entry.Order).ToList()
                    .Select(entry => new ProductPartialViewModel()
                    {
                        Product = entry
                    }).ToList();
                mainPageViewModel.News = db.InfoPages.Where(entry => entry.IsNews && entry.IsActive)
                    .OrderByDescending(entry => entry.CreatedOn).Take(5).ToList();
                mainPageViewModel.News.ForEach(entry =>
                    entry.Name = entry.Name.Length > 75 ? entry.Name.Substring(0, 75) + "..." : entry.Name);
                mainPageViewModel.PrimaryBanners =
                    db.Banners.Where(entry => entry.BannerType == BannerType.PrimaryMainPage)
                        .OrderBy(entry => entry.Order)
                        .ToList();
                mainPageViewModel.MobileBanners =
                    db.Banners.Where(entry => entry.BannerType == BannerType.MobileMainPage)
                        .OrderBy(entry => entry.Order)
                        .ToList();
                mainPageViewModel.TopSideBanners =
                    db.Banners.Where(entry => entry.BannerType == BannerType.SideTopMainPage)
                        .OrderBy(entry => entry.Order)
                        .ToList();
                mainPageViewModel.BottomSideBanners =
                    db.Banners.Where(entry => entry.BannerType == BannerType.SideBottomMainPage)
                        .OrderBy(entry => entry.Order)
                        .ToList();
                mainPageViewModel.FirstRowBanner = db.Banners.FirstOrDefault(entry => entry.BannerType == BannerType.FirstRowMainPage);
                mainPageViewModel.SecondRowBanner = db.Banners.FirstOrDefault(entry => entry.BannerType == BannerType.SecondRowMainPage);
                mainPageViewModel.Brands = db.Sellers.Include(entry => entry.Images).Where(entry => entry.IsActive && entry.IsFeatured).ToList();
            }
            return View(mainPageViewModel);
        }

        public ActionResult GettingBetter()
        {
            return View();
        }

        public ActionResult LoginPartial()
        {
            return PartialView("_LoginPartial");
        }

        public ActionResult MobileLoginPartial()
        {
            return PartialView("_MobileLoginPartial");
        }

        [HttpGet]
        public ActionResult SearchRegion(string query, int? minLevel = 4)
        {
            query = query.ToLower();
            using (var db = new ApplicationDbContext())
            {
                var regions =
                    db.Regions.Where(
                        entry =>
                            (entry.RegionLevel >= minLevel || entry.RegionLevel == 0) &&
                            (entry.Name_ru.ToLower().Contains(query) || entry.Name_ua.ToLower().Contains(query)))
                        .OrderBy(entry => entry.RegionLevel)
                        .Take(50)
                        .ToList();
                var result = new AutocompleteSearch
                {
                    query = query,
                    suggestions = regions.Select(entry => new ValueData()
                    {
                        value = entry.ExpandedName,
                        data = entry.Id.ToString()
                    }).ToArray()
                };
                return Json(result,
                    JsonRequestBehavior.AllowGet);
            }
        }

        public void uploadnow(HttpPostedFileWrapper upload)
        {
            if (upload != null)
            {
                string ImageName = upload.FileName;
                string path = Path.Combine(Server.MapPath("~/Images/uploads"), ImageName);
                upload.SaveAs(path);
            }
        }

        public ActionResult uploadPartial()
        {
            var appData = Server.MapPath("~/Images/uploads");
            var images = Directory.GetFiles(appData).Select(x => new ImagesViewModel
            {
                Url = Url.Content("/images/uploads/" + Path.GetFileName(x))
            });
            return View(images);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UploadImage(HttpPostedFileBase upload, string CKEditorFuncNum, string CKEditor,
            string langCode)
        {
            string url; // url to return
            string message; // message to display (optional)

            // here logic to upload image
            // and get file path of the image

            // path of the image
            string path = Server.MapPath("~/Images/uploads/") + upload.FileName;
            upload.SaveAs(path);

            url = Request.Url.GetLeftPart(UriPartial.Authority) + "/Images/uploads/" + upload.FileName;

            // passing message success/failure
            message = "Image was saved correctly";

            // since it is an ajax request it requires this string
            string output = @"<html><body><script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", \"" +
                            url + "\", \"" + message + "\");</script></body></html>";
            return Content(output);
        }

        [FetchSeller]
        [FetchCategories]
        public ActionResult Contacts()
        {
            return View("~/Views/SellerArea/Contacts.cshtml");
        }

        [FetchSeller]
        [FetchCategories]
        public ActionResult Reviews()
        {
            return View("~/Views/SellerArea/Reviews.cshtml");
        }

        [FetchSeller]
        [FetchCategories]
        public ActionResult catalog()
        {
            return View("~/Views/SellerArea/Catalog.cshtml");
        }

        [FetchSeller]
        [FetchCategories]
        public ActionResult gallery()
        {
            return View("~/Views/SellerArea/Gallery.cshtml");
        }

        public ActionResult Map()
        {
            /*
                        using (var db = new ApplicationDbContext())
                        {
                            var regionId = RegionService.GetRegionId();
                            var cats =
                                db.Sellers
                                    .Include(entry => entry.SellerCategories.Select(sc => sc.Category.ParentCategory))
                                    .Where(entry => entry.Addresses.Any(addr => addr.RegionId == regionId))
                                    .Select(entry => entry.SellerCategories.FirstOrDefault(cat => cat.IsDefault).Category)
                                     .OrderBy(
                                        entry =>
                                            entry.ParentCategory == null
                                                ? 1000
                                                : entry.ParentCategory.ParentCategory == null
                                                    ? 1000
                                                    : entry.ParentCategory.ParentCategory.Order)
                                    .ThenBy(
                                        entry => entry.ParentCategory == null ? 1000 : entry.ParentCategory.Order)
                                    .ThenBy(entry => entry.Order)
                                    .Where(entry => entry != null)
                                    .Distinct()
                                    .ToList();
                            return View(cats);
                        }
            */
            return View();
        }

        [HttpGet]
        public ActionResult GetMapData()
        {
            using (var db = new ApplicationDbContext())
            {
                var sellers =
                    db.Sellers
                    .Include(entry => entry.SellerCategories.Select(cat => cat.Category))
                    .Where(entry => entry.IsActive && entry.Longitude != null && entry.Latitude != null && entry.IsBenefitCardActive)
                    .ToList();

                var result = sellers.Select(entry => new SellerMapLocation()
                {
                    Name = entry.Name,
                    Url = Url.SubdomainAction(entry.UrlName, "Index", "Home"),
                    Specialization = entry.SellerCategories.FirstOrDefault(cat => cat.IsDefault) == null ? "" : entry.SellerCategories.FirstOrDefault(cat => cat.IsDefault).Category.Name,
                    UserDiscount = entry.UserDiscount,
                    Latitude = entry.Latitude.Value,
                    Longitude = entry.Longitude.Value
                }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [FetchLastNews]
        [FetchCategories]
        public ActionResult Anketa(SellerStatus status)
        {
            return View(new AnketaViewModel { Status = status });
        }

        [FetchLastNews]
        [FetchCategories]
        [HttpPost]
        public ActionResult Anketa(AnketaViewModel anketa)
        {
            if (ModelState.IsValid)
            {
                var emailService = new EmailService();
                emailService.SendSellerApplication(anketa);
                TempData["SuccessMessage"] = "Дякуюємо за вашу заявку, в найближчий час з вами зв'яжеться наш менеджер";
            }
            return View(anketa);
        }

        [FetchSeller]
        [FetchCategories]
        public ActionResult About()
        {
            return View("~/Views/SellerArea/About.cshtml");
        }
    }
}