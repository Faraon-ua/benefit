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
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Models;
using WebGrease.Css.Extensions;

namespace Benefit.Web.Controllers
{
    public class HomeController : BaseController
    {
        //        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Any, Duration = CacheConstants.OutputCacheLength)]
        public async Task<ActionResult> Index()
        {
            return View();
        }

        public ActionResult LoginPartial()
        {
            return PartialView("_LoginPartial");
        }

        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Any, Duration = CacheConstants.OutputCacheLength, VaryByParam = "parentCategoryId;isDropDown;")]
        public ActionResult CategoriesPartial(string parentCategoryId = null, bool? isDropDown = null)
        {
            using (var db = new ApplicationDbContext())
            {
                var parent = db.Categories.Find(parentCategoryId);
                var parentName = parent == null ? null : parent.Name;
                var categories = db.Categories.Include("ChildCategories").Include(entry => entry.ParentCategory).Where(entry => entry.ParentCategoryId == parentCategoryId && entry.IsActive).OrderBy(entry => entry.Order).ToList();
                if (parent != null)
                {
                    categories =
                        categories.Where(entry => !entry.ParentCategory.ChildAsFilters).ToList();
                }
                ViewBag.IsDropDown = isDropDown ?? false;
                return PartialView("_CategoriesPartial", new KeyValuePair<string, IEnumerable<Category>>(parentName, categories));
            }
        }

        [HttpGet]
        public ActionResult SearchRegion(string query)
        {
            query = query.ToLower();
            using (var db = new ApplicationDbContext())
            {
                var regions =
                    db.Regions.Where(
                        entry =>
                            (entry.RegionLevel >= 4 || entry.RegionLevel == 0) &&
                            (entry.Name_ru.ToLower().Contains(query) || entry.Name_ua.ToLower().Contains(query)))
                        .OrderBy(entry => entry.RegionLevel)
                        .Take(10)
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