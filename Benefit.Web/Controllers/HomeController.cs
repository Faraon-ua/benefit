using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Web.Models;

namespace Benefit.Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult SearchRegion(string search, int minLevel)
        {
            search = search.ToLower();
            using (var db = new ApplicationDbContext())
            {
                var regions = db.Regions.Where(entry => entry.RegionLevel >= minLevel && (entry.Name_ru.ToLower().Contains(search) || entry.Name_ua.ToLower().Contains(search))).OrderBy(entry => entry.RegionLevel).Take(10).ToList();
                return Json(regions.Select(entry => new { entry.Id, entry.ExpandedName }).ToList(), JsonRequestBehavior.AllowGet);
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
        public ActionResult UploadImage(HttpPostedFileBase upload, string CKEditorFuncNum, string CKEditor, string langCode)
        {
            string url; // url to return
            string message; // message to display (optional)

            // here logic to upload image
            // and get file path of the image

            // path of the image
            string path = "/Images/uploads/" + upload.FileName;

            url = Request.Url.GetLeftPart(UriPartial.Authority) + "/" + path;

            // passing message success/failure
            message = "Image was saved correctly";

            // since it is an ajax request it requires this string
            string output = @"<html><body><script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", \"" + url + "\", \"" + message + "\");</script></body></html>";
            return Content(output);
        }

        public ActionResult Index()
        {
            return View();
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
    }
}