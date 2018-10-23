using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;

namespace Benefit.Web.Controllers
{
    public class NewsController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public ActionResult Index()
        {
            var seller = ViewBag.Seller as Seller;
            var news = db.InfoPages.Where(entry => entry.IsNews && entry.IsActive)
                .OrderByDescending(entry => entry.CreatedOn).Take(ListConstants.NewsTakePerPage);
            if (seller != null)
            {
                news = news.Where(entry => entry.SellerId == seller.Id);
            }
            else
            {
                news = news.Where(entry => entry.SellerId == null);
            }
            ViewBag.PagesCount = db.InfoPages.Count(entry => entry.IsNews) / ListConstants.NewsTakePerPage;
            if (seller!=null)
            {
                var viewPath = string.Format("~/views/sellerarea/{0}/news.cshtml",
                    (ViewBag.Seller as Seller).EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default)
                    .ToString());
                return View(viewPath, news.ToList());
            }
            return View(news.ToList());
        }

        public ActionResult FetchNews(int page = 0)
        {
            var news = db.InfoPages.Where(entry => entry.IsNews && entry.IsActive).OrderByDescending(entry => entry.CreatedOn).Skip(page * ListConstants.NewsTakePerPage).Take(ListConstants.NewsTakePerPage).ToList();
            var html = string.Join("", news.Select(entry => ControllerContext.RenderPartialToString("_PageBlock", entry)));
            return Content(html);
        }
    }
}