using System.Web.Mvc;
using Benefit.Domain.Models;
using Benefit.Web.Filters;

namespace Benefit.Web.Controllers
{
    public class ErrorController : Controller
    {
        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public ActionResult NotFound()
        {
            var seller = ViewBag.Seller as Seller;
            if (seller != null && seller.HasEcommerce)
            {
                var viewName = string.Format("~/views/sellerarea/{0}/notfound.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return View(viewName, seller);
            }
            return View();

        }
    }
}
