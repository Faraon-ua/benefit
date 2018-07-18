using System.Web.Mvc;
using System.Web.Routing;
using Benefit.Services;
using Benefit.Web.Helpers;

namespace Benefit.Web.Filters
{
    public class AccessPasswordAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if(RouteDataHelper.ControllerName == "home" && RouteDataHelper.ActionName == "passwordprotect")
                return;
            if (filterContext.RequestContext.HttpContext.Session["AccessPassword"] != null && (bool)filterContext.RequestContext.HttpContext.Session["AccessPassword"])
                return;
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                    { "controller", "Home" },
                    { "action", "PasswordProtect" }
                });
            base.OnActionExecuting(filterContext);
        }
    }
}