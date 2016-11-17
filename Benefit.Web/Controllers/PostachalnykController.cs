using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.Models;
using Benefit.Services.Domain;

namespace Benefit.Web.Controllers
{
    public class PostachalnykController : Controller
    {
        SellerService SellerService = new SellerService();
        //
        // GET: /Postachalnyk/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Info(string id)
        {
            var referrer = Request.UrlReferrer;
            string categoryUrlName = null;
            if (referrer != null && referrer.PathAndQuery.Contains(RouteConstants.CategoriesRoutePrefix))
            {
                categoryUrlName =
                    referrer.PathAndQuery.Substring(
                        referrer.PathAndQuery.IndexOf(RouteConstants.CategoriesRoutePrefix) + RouteConstants.CategoriesRoutePrefix.Length + 1);
            }
            var sellerVm = SellerService.GetSellerDetails(id, categoryUrlName);
            if (sellerVm.Seller == null) return HttpNotFound();
            return View(sellerVm);
        }

        public ActionResult Catalog(string sellerUrl = null, string categoryUrl = null)
        {
            var model = SellerService.GetSellerCatalog(sellerUrl, categoryUrl);
            return View("../Catalog/ProductsCatalog", model);
        }
    }
}