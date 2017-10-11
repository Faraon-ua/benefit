using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;

namespace Benefit.Web.Controllers
{
    public class PagesController : Controller
    {
        //
        // GET: /Pages/
        ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index(string id)
        {
            var page = db.InfoPages.FirstOrDefault(entry => entry.UrlName == id);
            if (page == null) return HttpNotFound();
            return View(page);
        }

        public ActionResult Content(string id)
        {
            var page = db.InfoPages.FirstOrDefault(entry => entry.Id == id);
            if (page == null) return HttpNotFound();
            return View(page);
        }

        public ActionResult News(int page = 0)
        {
            var news = db.InfoPages.Where(entry => entry.IsNews && entry.IsActive).OrderByDescending(entry => entry.CreatedOn).Skip(page * ListConstants.NewsTakePerPage).Take(ListConstants.NewsTakePerPage).ToList();
            ViewBag.PagesCount = db.InfoPages.Count(entry => entry.IsNews) / ListConstants.NewsTakePerPage;
            return View(news);
        }
    }
}