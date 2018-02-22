using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Web.Filters;

namespace Benefit.Web.Controllers
{
    public class PagesController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        [FetchCategories]
        [FetchLastNews]
        public ActionResult Index(string id)
        {
            var page = db.InfoPages.FirstOrDefault(entry => entry.UrlName == id);
            if (page == null) return HttpNotFound();
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