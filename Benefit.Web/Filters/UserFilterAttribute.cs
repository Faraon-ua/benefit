using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Filters
{
    public class UserFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated &&
                filterContext.HttpContext.Request.Cookies[RouteConstants.FullNameCookieName] == null)
            {
                using (var db = new ApplicationDbContext())
                {
                    var user = db.Users.Find(filterContext.RequestContext.HttpContext.User.Identity.GetUserId());
                    var cookieValue = HttpUtility.UrlEncode(user.FullName);
                    var fullNameCookie = new HttpCookie(RouteConstants.FullNameCookieName, cookieValue)
                    {
                        Expires = DateTime.UtcNow.AddYears(1)
                    };
                    HttpContext.Current.Response.Cookies.Add(fullNameCookie);
                }
            }

            if (filterContext.RouteData.DataTokens["area"] != null &&
                filterContext.RouteData.DataTokens["area"] == "Admin")
            {
                //if new sellerId provided but old sellerId is in Session - clear session
                var urlSellerId = filterContext.HttpContext.Request.QueryString[DomainConstants.SellerSessionIdKey];
                var currentSellerId = filterContext.HttpContext.Session[DomainConstants.SellerSessionIdKey];
                if (urlSellerId != null && currentSellerId != null)
                {
                    currentSellerId = null;
                }
                // if not admin AND if seller AND if seller belongst to currentUser
                if (!filterContext.HttpContext.User.IsInRole(DomainConstants.AdminRoleName) &&
                    filterContext.HttpContext.User.IsInRole(DomainConstants.SellerRoleName) &&
                    currentSellerId == null)
                {
                    using (var db = new ApplicationDbContext())
                    {
                        var userId = filterContext.HttpContext.User.Identity.GetUserId();
                        Seller seller = null;
                        seller = urlSellerId != null
                            ? db.Sellers.FirstOrDefault(entry => entry.OwnerId == userId && entry.Id == urlSellerId)
                            : db.Sellers.FirstOrDefault(entry => entry.OwnerId == userId);

                        filterContext.HttpContext.Session.Add(DomainConstants.SellerSessionIdKey, seller.Id);
                        filterContext.HttpContext.Session.Add(DomainConstants.SellerSessionNameKey, seller.Name);
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}