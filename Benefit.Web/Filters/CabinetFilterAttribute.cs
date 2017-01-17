using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Filters
{
    public class CabinetFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.RequestContext.HttpContext.Request.QueryString.AllKeys.Contains(DomainConstants.UserIdKey)
                && filterContext.RequestContext.HttpContext.User.IsInRole(DomainConstants.AdminRoleName))
            {
                filterContext.RouteData.Values.Add(DomainConstants.UserIdKey,
                    filterContext.RequestContext.HttpContext.Request.QueryString[DomainConstants.UserIdKey]);
            }
            else
            {
                filterContext.RouteData.Values.Add(DomainConstants.UserIdKey, filterContext.RequestContext.HttpContext.User.Identity.GetUserId());
            }
            base.OnActionExecuting(filterContext);
        }
    }
}