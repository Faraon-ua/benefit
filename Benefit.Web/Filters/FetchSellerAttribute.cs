using System.Web.Mvc;
using Benefit.Services.Domain;

namespace Benefit.Web.Filters
{
    public class FetchSellerAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var subdomain = filterContext.RequestContext.RouteData.Values["subdomain"];
            if (subdomain != null)
            {
                var sellerService = new SellerService();
                var seller = sellerService.GetSeller(subdomain.ToString());
                filterContext.Controller.ViewBag.Seller = seller;
            }
        }
    }
}