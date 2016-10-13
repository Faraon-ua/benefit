using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Filters
{
    public class UserFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            using (var db = new ApplicationDbContext())
            {
                if (filterContext.HttpContext.Request.IsAuthenticated &&
                    filterContext.HttpContext.Request.Cookies[RouteConstants.FullNameCookieName] == null)
                {
                    var user = db.Users.Find(filterContext.RequestContext.HttpContext.User.Identity.GetUserId());
                    var cookieValue = HttpUtility.UrlEncode(user.FullName);
                    var fullNameCookie = new HttpCookie(RouteConstants.FullNameCookieName, cookieValue)
                    {
                        Expires = DateTime.UtcNow.AddYears(1)
                    };
                    HttpContext.Current.Response.Cookies.Add(fullNameCookie);
                }

                if (filterContext.RouteData.DataTokens["area"] != null &&
                    filterContext.RouteData.DataTokens["area"] == "Admin")
                {
                    if (filterContext.HttpContext.User.IsInRole(DomainConstants.SellerRoleName) &&
                        filterContext.HttpContext.Session[DomainConstants.SellerSessionIdKey] == null)
                    {
                        var userId = filterContext.HttpContext.User.Identity.GetUserId();
                        filterContext.HttpContext.Session.Add(DomainConstants.SellerSessionIdKey,
                            db.Sellers.FirstOrDefault(entry => entry.OwnerId == userId).Id);
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}