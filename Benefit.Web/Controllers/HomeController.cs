using System;
using System.Collections;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;
using Benefit.Services;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;
using Benefit.Web.Models;
using Benefit.Web.Models.ViewModels;
using NLog;

namespace Benefit.Web.Controllers
{
    public class HomeController : BaseController
    {
        private Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        [FetchSeller(Order = 0, Include = "Banners")]
        [FetchCategories(Order = 1)]
        //[OutputCache(Location = System.Web.UI.OutputCacheLocation.Any, Duration = CacheConstants.OutputCacheLength, VaryByCustom = "IsMobile")]
        public async Task<ActionResult> Index()
        {
            var productsDbContext = new ProductsDBContext();
            var bannersDbContext = new BannersDBContext();
            var infoPagesDBContext = new InfoPagesDBContext();
            var sellersDBContext = new SellersDBContext();
            //handle seller subdomain
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                using (var db = new ApplicationDbContext())
                {
                    seller.FeaturedProducts = db.Products
                        .Include(entry => entry.Favorites)
                        .Include(entry => entry.Images)
                        .Include(entry => entry.Category)
                        .Include(entry => entry.Seller)
                        .Include(entry => entry.Seller.ShippingMethods.Select(sm => sm.Region))
                        .Where(entry =>
                            entry.IsActive && entry.SellerId == seller.Id && entry.IsFeatured).Take(8).ToList();
                    seller.NewProducts =
                        db.Products
                        .Include(entry => entry.Favorites)
                        .Include(entry => entry.Images)
                        .Include(entry => entry.Category)
                        .Include(entry => entry.Seller)
                        .Include(entry => entry.Seller.ShippingMethods.Select(sm => sm.Region))
                        .Where(entry =>
                            entry.IsActive && entry.SellerId == seller.Id && entry.IsNewProduct).Take(8).ToList();
                    seller.PromotionProducts = db.Products
                        .Include(entry => entry.Favorites)
                        .Include(entry => entry.Images)
                        .Include(entry => entry.Category)
                        .Include(entry => entry.Seller)
                        .Include(entry => entry.Seller.ShippingMethods.Select(sm => sm.Region))
                        .Where(entry =>
                            entry.IsActive &&
                            entry.ModerationStatus == ModerationStatus.Moderated &&
                            entry.AvailableAmount > 0 &&
                            (entry.AvailabilityState == ProductAvailabilityState.AlwaysAvailable || entry.AvailabilityState == ProductAvailabilityState.Available) && 
                            entry.Category.IsActive && 
                            entry.SellerId == seller.Id && 
                            entry.OldPrice != null).Take(8).ToList();
                    foreach (var featuredProduct in seller.FeaturedProducts)
                    {
                        if (featuredProduct.Currency != null)
                        {
                            featuredProduct.Price = featuredProduct.Price * featuredProduct.Currency.Rate;
                        }
                    }
                    foreach (var newProduct in seller.NewProducts)
                    {
                        if (newProduct.Currency != null)
                        {
                            newProduct.Price = newProduct.Price * newProduct.Currency.Rate;
                        }
                    }
                    foreach (var promotionProduct in seller.PromotionProducts)
                    {
                        if (promotionProduct.Currency != null)
                        {
                            promotionProduct.Price = promotionProduct.Price * promotionProduct.Currency.Rate;
                        }
                    }
                }

                if (seller.HasEcommerce)
                {
                    var viewName = string.Format("~/views/sellerarea/{0}/home.cshtml",
                        seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                    return View(viewName, seller);
                }
                return View("~/views/sellerareabc/home.cshtml", seller);
            }
            var mainPageViewModel = new MainPageViewModel();
            var products = productsDbContext.GetMainPageProducts();
            var banners = bannersDbContext.Get("BannerType != 1 and SellerId is null");
            var pages = infoPagesDBContext.GetMainPagesNews();
            var sellers = sellersDBContext.GetBrands();

            mainPageViewModel.FeaturedProducts = (from element in products.Where(entry => entry.IsFeatured)
                                                  group element by element.SellerId
                                                  into groups
                                                  select groups.OrderBy(p => Guid.NewGuid()).FirstOrDefault()).ToList();
            mainPageViewModel.NewProducts = (from element in products.Where(entry => entry.IsNewProduct)
                                             group element by element.SellerId
                                                 into groups
                                             select groups.OrderBy(p => Guid.NewGuid()).FirstOrDefault()).ToList();

            mainPageViewModel.PrimaryBanners = banners.Where(entry => entry.BannerType == BannerType.PrimaryMainPage)
                    .OrderBy(entry => entry.Order)
                    .ToList();
            mainPageViewModel.MobileBanners = banners.Where(entry => entry.BannerType == BannerType.MobileMainPage)
                    .OrderBy(entry => entry.Order)
                    .ToList();
            mainPageViewModel.TopSideBanners = banners.Where(entry => entry.BannerType == BannerType.SideTopMainPage)
                    .OrderBy(entry => entry.Order)
                    .ToList();
            mainPageViewModel.BottomSideBanners =
                banners.Where(entry => entry.BannerType == BannerType.SideBottomMainPage)
                    .OrderBy(entry => entry.Order)
                    .ToList();
            mainPageViewModel.FirstRowBanner = banners.FirstOrDefault(entry => entry.BannerType == BannerType.FirstRowMainPage);
            mainPageViewModel.SecondRowBanner = banners.FirstOrDefault(entry => entry.BannerType == BannerType.SecondRowMainPage);

            mainPageViewModel.News = pages.Where(entry => entry.IsNews).ToList();
            mainPageViewModel.News.ForEach(entry =>
                entry.Name = entry.Name.Length > 75 ? entry.Name.Substring(0, 75) + "..." : entry.Name);
            mainPageViewModel.Description = pages.FirstOrDefault(entry => entry.UrlName == "golovna");
            mainPageViewModel.Brands = sellers.ToList();

            return View(mainPageViewModel);
        }

        public ActionResult GettingBetter()
        {
            var cacheSize = 0;
            BinaryFormatter bf = new BinaryFormatter();
            var sb = new StringBuilder();
            sb.Append("Cache objects:\n");
            using (var ms = new MemoryStream())
            {
                foreach (var c in HttpRuntime.Cache)
                {
                    var obj = HttpRuntime.Cache[((DictionaryEntry)c).Key.ToString()];
                    sb.Append(((DictionaryEntry)c).Key + " - ");
                    try
                    {
                        bf.Serialize(ms, obj);
                        sb.Append(ms.Length);
                        sb.Append(" bytes");
                    }
                    catch (Exception e)
                    {
                        sb.Append(e.ToString());
                    }
                    sb.Append(Environment.NewLine);
                }
                _logger.Info(sb);
            }
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

        [FetchSeller(Order = 0, Include = "Schedules")]
        [FetchCategories(Order = 1)]
        public ActionResult Contacts()
        {
            var seller = ViewBag.Seller as Seller;
            var viewName = string.Format("~/views/sellerarea/{0}/Contacts.cshtml",
                seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
            return View(viewName, seller);
        }

        [FetchSeller(Order = 0, Include = "Reviews")]
        [FetchCategories(Order = 1)]
        public ActionResult Reviews()
        {
            var seller = ViewBag.Seller as Seller;
            var layoutName = string.Format("~/Views/SellerArea/{0}/Reviews.cshtml",
                seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
            return View(layoutName);
        }

        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public ActionResult catalog()
        {
            var seller = ViewBag.Seller as Seller;
            var viewName = string.Format("~/views/sellerarea/{0}/Catalog.cshtml",
                seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
            return View(viewName, seller);
        }

        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public ActionResult gallery()
        {
            var seller = ViewBag.Seller as Seller;
            var viewName = string.Format("~/views/sellerarea/{0}/Gallery.cshtml",
                seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
            return View(viewName, seller);
        }

        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public ActionResult delivery()
        {
            var seller = ViewBag.Seller as Seller;
            var viewName = string.Format("~/views/sellerarea/{0}/Delivery.cshtml",
                seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
            return View(viewName, seller);
        }

        [FetchCategories(Order = 1)]
        public ActionResult Map()
        {
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
        [FetchCategories(Order = 1)]
        public ActionResult Anketa(SellerStatus status)
        {
            return View(new AnketaViewModel { Status = status });
        }

        [FetchLastNews]
        [FetchCategories(Order = 1)]
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

        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public ActionResult About()
        {
            var seller = ViewBag.Seller as Seller;
            var layoutName = string.Format("~/Views/SellerArea/{0}/About.cshtml",
                seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
            return View(layoutName);
        }

        [ValidateInput(false)]
        public ActionResult ShowMessagePopup(string message, bool showButtons)
        {
            return PartialView("_Message", new MessagePopupViewModel()
            {
                Message = message,
                ShowButtons = showButtons
            });
        }

        public ActionResult PasswordProtect()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PasswordProtect(string password)
        {
            if (password == "aabbccee")
            {
                Session["AccessPassword"] = true;
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("password", "пароль невірний");
            return View();
        }

        [HttpGet]
        public ActionResult Pivacy()
        {
            return View();
        }
    }
}