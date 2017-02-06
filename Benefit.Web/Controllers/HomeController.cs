using System;
using System.Collections.Generic;
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
using Benefit.Services.Domain;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Models;
using WebGrease.Css.Extensions;

namespace Benefit.Web.Controllers
{
    public class HomeController : BaseController
    {
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Any, Duration = CacheConstants.OutputCacheLength)]
        public async Task<ActionResult> Index()
        {
            var hitIds = new List<string>
            {
                "e5461d6c-abce-11e5-bbba-e03f49eb1351",
                "7334de7a-ad63-11e5-bbba-e03f49eb1351",
                "32fb0f8a-28c6-11e6-b8b4-e03f49eb1351",
                "88efb80c-36e4-11e5-a8bf-10feed06278f",
                "09049d8c-36de-11e5-a8bf-10feed06278f",
                "7580c148-d55f-4009-b361-a5a99fb0a767",
                "d8874ed3-930a-4743-ac7b-af0efce31db1",
                "f48fcde5-69f2-4124-9d8c-7375ee67cc9d",
                "1b290794-eb6c-4a55-bb3e-48980acc29a3"
            };
            var newIds = new List<string>
            {
                "64892da0-38b4-409e-bcd8-ee43285e2608",
                "0a7dc739-2f59-41a2-8fef-aded77dadfd4",
                "910fd782-75a3-43eb-b963-fc8880e3d40a",
                "5c24366d-1e20-440b-8637-cc6382f3d4f2",
                "bb192c1c-9190-48e5-91e9-82f239dd29bc",
                "683770ff-1d63-43de-9151-fc6f2560d03f",
                "3bb7c047-a8d2-414d-9e79-a2a026a83dd1",
                "0c2df8cb-906b-4eee-a477-cc0bf9f78e3b",
                "86e91251-811d-4a9a-8dc4-d29ac1891bf7"
            };
            var mainPageViewModel = new MainPageViewModel();
            using (var db = new ApplicationDbContext())
            {
                mainPageViewModel.BestSellers = db.Products
                    .Include(entry => entry.Images)
                    .Include(entry => entry.Category)
                    .Include(entry => entry.Seller)
                    .Where(entry => hitIds.Contains(entry.Id)).ToList().OrderBy(entry=>hitIds.IndexOf(entry.Id)).ToList();
                mainPageViewModel.NewProducts = db.Products
                    .Include(entry => entry.Images)
                    .Include(entry => entry.Category)
                    .Include(entry => entry.Seller)
                    .Where(entry => newIds.Contains(entry.Id)).ToList().OrderBy(entry => newIds.IndexOf(entry.Id)).ToList();
                mainPageViewModel.Banners =
                    db.Banners.Where(entry => entry.BannerType == BannerType.MainPageBanners)
                        .OrderBy(entry => entry.Order)
                        .ToList();
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

        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Any, Duration = CacheConstants.OutputCacheLength, VaryByParam = "parentCategoryId;sellerUrl;isDropDown;")]
        public ActionResult CategoriesPartial(string parentCategoryId, string sellerUrl, bool? isDropDown = null)
        {
            if (string.IsNullOrEmpty(parentCategoryId))
                parentCategoryId = null;
            if (string.IsNullOrEmpty(sellerUrl))
                sellerUrl = null;
            using (var db = new ApplicationDbContext())
            {
                var parent = db.Categories.Find(parentCategoryId);
                var parentName = parent == null ? null : parent.Name;
                var categories = db.Categories.Include(entry => entry.ChildCategories).Include(entry => entry.ParentCategory).Where(entry => entry.ParentCategoryId == parentCategoryId && entry.IsActive).OrderBy(entry => entry.Order).ToList();
                if (sellerUrl != null)
                {
                    var sellersService = new SellerService();
                    var sellerCategories = sellersService.GetAllSellerCategories(sellerUrl).ToList();
                    var sellerCategoriesIds =
                        sellersService.GetAllSellerCategories(sellerUrl).Select(entry => entry.Id).ToList();
                    if (parent == null || parent.ParentCategoryId == null)
                    {
                        categories =
                            sellerCategories.Where(entry => entry.ParentCategoryId == null)
                                .OrderBy(entry => entry.Order)
                                .ToList();
                        foreach (var category in categories)
                        {
                            category.ChildCategories = category.ChildCategories.OrderBy(entry => entry.Order).ToList();
                        }
                        if (parent == null)
                        {
                            parentName = categories.FirstOrDefault() == null ? null : categories.FirstOrDefault().Name;
                        }
                    }
                    else
                    {
                        categories = categories.Where(entry => sellerCategoriesIds.Contains(entry.Id)).ToList();
                        while (categories.Count == 1)
                        {
                            var catId = categories.First().Id;
                            parentName = categories.First().Name;
                            var childCategories =
                                db.Categories.Include(entry => entry.ParentCategory)
                                    .Where(
                                        entry =>
                                            entry.ParentCategoryId == catId && entry.IsActive &&
                                            sellerCategoriesIds.Contains(entry.Id))
                                    .OrderBy(entry => entry.Order)
                                    .ToList();
                            if (childCategories.Count == 0) break;
                            categories = childCategories;
                        }

                    }
                    var sellerCatsModel = new CategoriesListViewModel()
                    {
                        ParentName = parentName,
                        SellerUrlName = sellerUrl,
                        Items = categories.ToList()
                    };
                    return PartialView((parent == null || parent.ParentCategoryId == null) ? "_SellerCategoriesPartial" : "_SellerChildCategoriesPartial", sellerCatsModel);
                }
                if (parent != null)
                {
                    categories = categories.Where(entry => !entry.ParentCategory.ChildAsFilters).ToList();
                }

                ViewBag.IsDropDown = isDropDown ?? false;
                var model = new CategoriesListViewModel()
                {
                    ParentName = parentName,
                    Items = categories.ToList()
                };

                return PartialView("_CategoriesPartial", model);
            }
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
                        .Take(15)
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

            url = Request.Url.GetLeftPart(UriPartial.Authority) + "/Images/uploads/" + upload.FileName; ;

            // passing message success/failure
            message = "Image was saved correctly";

            // since it is an ajax request it requires this string
            string output = @"<html><body><script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", \"" +
                            url + "\", \"" + message + "\");</script></body></html>";
            return Content(output);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        #region tempMethods

        public ActionResult PostExportActions()
        {
            using (var db = new ApplicationDbContext())
            {
                //string empty card numbers to null
                db.Users.Where(entry => entry.CardNumber == "").ForEach(entry =>
                {
                    entry.CardNumber = null;
                    db.Entry(entry).State = EntityState.Modified;
                });

                //decode seller descriptions
                foreach (var seller in db.Sellers)
                {
                    seller.Description = Server.HtmlDecode(seller.Description);
                    seller.Name = seller.Name.Replace("&quot;", string.Empty);
                    db.Entry(seller).State = EntityState.Modified;
                }

                db.SaveChanges();

                //resave seller images

                #region ftp

                /* using (var ftpClient = new FtpClient())
                {
                    ftpClient.Host = "benefit-company.com";
                    ftpClient.Credentials = new NetworkCredential("test1", "ynYjAQJs");
                    ftpClient.ReadTimeout = 50000;
                    ftpClient.DataConnectionConnectTimeout = 50000;
                    ftpClient.DataConnectionReadTimeout = 50000;

                    var imagesPath = "/www/benefit-company.com/image/";
                    var destinationDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty),"Images");

                    ftpClient.Connect();

                    foreach (var sellerImage in db.Images.Where(entry => entry.SellerId != null))
                    {
                        using (
                            var ftpStream =
                                ftpClient.OpenRead(Path.Combine(imagesPath, sellerImage.ImageUrl)))
                        {
                            var dotIndex = sellerImage.ImageUrl.LastIndexOf('.');
                            var fileExt = sellerImage.ImageUrl.Substring(dotIndex,
                                sellerImage.ImageUrl.Length - dotIndex);

                            var imagePath = string.Empty;
                            if (sellerImage.ImageType == ImageType.SellerLogo)
                            {
                                imagePath = Path.Combine(destinationDirectory, sellerImage.ImageType.ToString(), sellerImage.SellerId + fileExt);
                            }
                            if (sellerImage.ImageType == ImageType.SellerGallery)
                            {
                                imagePath = Path.Combine(destinationDirectory, sellerImage.ImageType.ToString(), sellerImage.SellerId, sellerImage.Id + fileExt);
                            }
                            using (
                                var fileStream =
                                    System.IO.File.Create(imagePath, (int) ftpStream.Length))
                            {
                                var buffer = new byte[8*1024];
                                int count;
                                while ((count = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fileStream.Write(buffer, 0, count);
                                }
                            }
                            sellerImage.ImageUrl = sellerImage.SellerId + fileExt;
                            db.Entry(sellerImage).State = EntityState.Modified;
                        }
                    }

                    ftpClient.Disconnect();
                }*/

                #endregion

                #region Images local

                //var imagesPath = "D:/BenefitStuff/images/";
                //var destinationDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty), "Images");
                //foreach (var sellerImage in db.Images.Where(entry => entry.SellerId != null))
                //{
                //    var dotIndex = sellerImage.ImageUrl.LastIndexOf('.');
                //    var fileExt = sellerImage.ImageUrl.Substring(dotIndex,
                //        sellerImage.ImageUrl.Length - dotIndex);

                //    var destPath = string.Empty;
                //    var sourcePath = Path.Combine(imagesPath, sellerImage.ImageUrl);
                //    if (sellerImage.ImageType == ImageType.SellerLogo)
                //    {
                //        sellerImage.ImageUrl = sellerImage.SellerId + fileExt;
                //        destPath = Path.Combine(destinationDirectory, sellerImage.ImageType.ToString(), sellerImage.SellerId + fileExt);
                //    }
                //    if (sellerImage.ImageType == ImageType.SellerGallery)
                //    {
                //        Directory.CreateDirectory(Path.Combine(destinationDirectory, sellerImage.ImageType.ToString(), sellerImage.SellerId));
                //        sellerImage.ImageUrl = sellerImage.Id + fileExt;
                //        destPath = Path.Combine(destinationDirectory, sellerImage.ImageType.ToString(), sellerImage.SellerId, sellerImage.Id + fileExt);
                //    }
                //    if (System.IO.File.Exists(sourcePath))
                //    {
                //        System.IO.File.Copy(sourcePath, destPath, true);
                //        db.Entry(sellerImage).State = EntityState.Modified;
                //    }
                //}
                #endregion

                db.SaveChanges();
                return Content("ok");
            }

        #endregion
        }
    }
}