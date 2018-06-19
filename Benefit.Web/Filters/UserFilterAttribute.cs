using System;
using System.Linq;
using System.Data.Entity;
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
                    var cookieNameValue = HttpUtility.UrlEncode(user.FullName);
                    var cookieReferalValue = user.ExternalNumber.ToString();

                    var fullNameCookie = new HttpCookie(RouteConstants.FullNameCookieName, cookieNameValue)
                    {
                        Expires = DateTime.UtcNow.AddYears(1)
                    };
                    var selfReferalNumber = new HttpCookie(RouteConstants.SelfReferalCookieName, cookieReferalValue)
                    {
                        Expires = DateTime.UtcNow.AddYears(1)
                    };
                    HttpContext.Current.Response.Cookies.Add(fullNameCookie);
                    HttpContext.Current.Response.Cookies.Add(selfReferalNumber);
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
                // if not admin AND if seller AND if seller belongs to currentUser
                if (!filterContext.HttpContext.User.IsInRole(DomainConstants.AdminRoleName) &&
                    (filterContext.HttpContext.User.IsInRole(DomainConstants.SellerRoleName) || filterContext.HttpContext.User.IsInRole(DomainConstants.SellerOperatorRoleName) || filterContext.HttpContext.User.IsInRole(DomainConstants.SellerModeratorRoleName)) &&
                    currentSellerId == null)
                {
                    using (var db = new ApplicationDbContext())
                    {
                        var userId = filterContext.HttpContext.User.Identity.GetUserId();
                        Seller seller = null;
                        if (urlSellerId != null)
                        {
                            seller =
                                db.Sellers.FirstOrDefault(entry => entry.OwnerId == userId && entry.Id == urlSellerId) ??
                                db.Sellers.Include(entry => entry.Personnels)
                                    .FirstOrDefault(
                                        entry => entry.Personnels.Select(pers => pers.UserId).Contains(userId) && entry.Id == urlSellerId);
                        }
                        else
                        {
                            seller = db.Sellers.FirstOrDefault(entry => entry.OwnerId == userId) ??
                            db.Sellers.Include(entry => entry.Personnels)
                                    .FirstOrDefault(
                                        entry => entry.Personnels.Select(pers => pers.UserId).Contains(userId));
                        }

                        filterContext.HttpContext.Session.Add(DomainConstants.SellerSessionIdKey, seller.Id);
                        filterContext.HttpContext.Session.Add(DomainConstants.SellerSessionNameKey, seller.Name);
                        filterContext.HttpContext.Session.Add(DomainConstants.SellerEcommerceKey, seller.HasEcommerce);
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}