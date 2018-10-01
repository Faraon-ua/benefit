using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;

namespace Benefit.Web.Controllers
{
    public class NewsController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        [FetchCategories(Order = 1)]
        public ActionResult Index()
        {
            var news = db.InfoPages.Where(entry => entry.IsNews && entry.IsActive).OrderByDescending(entry => entry.CreatedOn).Take(ListConstants.NewsTakePerPage).ToList();
            ViewBag.PagesCount = db.InfoPages.Count(entry => entry.IsNews) / ListConstants.NewsTakePerPage;
            return View(news);
        }

        public ActionResult FetchNews(int page = 0)
        {
            var news = db.InfoPages.Where(entry => entry.IsNews && entry.IsActive).OrderByDescending(entry => entry.CreatedOn).Skip(page * ListConstants.NewsTakePerPage).Take(ListConstants.NewsTakePerPage).ToList();
            var html = string.Join("", news.Select(entry => ControllerContext.RenderPartialToString("_PageBlock", entry)));
            return Content(html);
        }
    }
}