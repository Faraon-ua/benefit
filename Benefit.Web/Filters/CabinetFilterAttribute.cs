using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Services.Domain;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Filters
{
    public class CabinetFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var userService = new UserService();
            if (filterContext.RouteData.Values[DomainConstants.UserIdKey] == null)
            {
                if (filterContext.RequestContext.HttpContext.Request.QueryString.AllKeys.Contains(
                    DomainConstants.UserIdKey)
                    && filterContext.RequestContext.HttpContext.User.IsInRole(DomainConstants.AdminRoleName))
                {
                    filterContext.RouteData.Values.Add(DomainConstants.UserIdKey,
                        filterContext.RequestContext.HttpContext.Request.QueryString[DomainConstants.UserIdKey]);
                    filterContext.Controller.ViewBag.User =
                        userService.GetUserInfoWithRegions(
                            filterContext.RouteData.Values[DomainConstants.UserIdKey].ToString());
                }
                else
                {

                    filterContext.Controller.ViewBag.User =
                        userService.GetUserInfoWithRegions(
                            filterContext.RequestContext.HttpContext.User.Identity.GetUserId());
                    filterContext.RouteData.Values.Add(DomainConstants.UserIdKey,
                        filterContext.RequestContext.HttpContext.User.Identity.GetUserId());

                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}