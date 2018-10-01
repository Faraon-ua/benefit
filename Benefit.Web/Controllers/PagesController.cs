using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Filters;

namespace Benefit.Web.Controllers
{
    public class PagesController : BaseController
    {
        ApplicationDbContext db = new ApplicationDbContext();

        [FetchSeller(Order=0)]
        [FetchCategories(Order = 1)]
        [FetchLastNews]
        public ActionResult Index(string id)
        {
            if (ViewBag.Seller != null)
            {
                var seller = ViewBag.Seller as Seller;
                var page = db.InfoPages.FirstOrDefault(entry => entry.UrlName == id && entry.SellerId == seller.Id);
                if (page == null) throw new HttpException(404, "Not found");
                var viewName = string.Format("~/views/sellerarea/{0}/Page.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return View(viewName, page);
            }
            else
            {
                var page = db.InfoPages.FirstOrDefault(entry => entry.UrlName == id);
                if (page == null) throw new HttpException(404, "Not found");
                return View(page);
            }
        }

        public ActionResult PageContent(string id)
        {
            var page = db.InfoPages.FirstOrDefault(entry => entry.Id == id);
            if (page == null) throw new HttpException(404, "Not found");
            return View(page);
        }
    }
}