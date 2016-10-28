using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Services.Domain;

namespace Benefit.Web.Controllers
{
    public class PostachalnykController : Controller
    {
        //
        // GET: /Postachalnyk/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Info(string id)
        {
            var sellerService = new SellerService();
            var referrer = Request.UrlReferrer;
            string categoryUrlName = null;
            if (referrer != null && referrer.PathAndQuery.Contains(RouteConstants.CategoriesRoutePrefix))
            {
                categoryUrlName =
                    referrer.PathAndQuery.Substring(
                        referrer.PathAndQuery.IndexOf(RouteConstants.CategoriesRoutePrefix) + RouteConstants.CategoriesRoutePrefix.Length + 1);
            }
            var sellerVm = sellerService.GetSellerDetails(id, categoryUrlName);
            if (sellerVm.Seller == null) return HttpNotFound();
            return View(sellerVm);
        }
    }
}