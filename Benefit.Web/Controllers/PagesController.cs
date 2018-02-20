using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;

namespace Benefit.Web.Controllers
{
    public class PagesController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        [FetchCategories]
        public ActionResult Index(string id)
        {
            var page = db.InfoPages.FirstOrDefault(entry => entry.UrlName == id);
            if (page == null) return HttpNotFound();
            ViewBag.LastNews = db.InfoPages.Where(entry => entry.IsNews && entry.IsActive).OrderByDescending(entry => entry.CreatedOn).Take(3).ToList();
            return View(page);
        }

        public ActionResult PageContent(string id)
        {
            var page = db.InfoPages.FirstOrDefault(entry => entry.Id == id);
            if (page == null) return HttpNotFound();
            return View(page);
        }
    }
}