using System.Net;
using System.Web.Mvc;
using Benefit.Web.Helpers;

namespace Benefit.Web.Filters
{
    public class PasswordFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if ((RouteDataHelper.ControllerName == "home" && RouteDataHelper.ActionName == "index") ||
                (RouteDataHelper.ControllerName == "account" && RouteDataHelper.ActionName == "login")) return;
            if (!(filterContext.RequestContext.HttpContext.User.Identity.IsAuthenticated &&
                  filterContext.RequestContext.HttpContext.User.IsInRole("Admin")))
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }
    }
}