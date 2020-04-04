using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Base;

namespace Benefit.Web.Filters
{
    public class FetchSellerAttribute : ActionFilterAttribute
    {
        public string Include { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var subdomain = filterContext.RequestContext.RouteData.Values["subdomain"];
            Seller seller;
            if (subdomain != null)
            {
                var cacheKey = string.Format("Seller[{0}]With({1})", subdomain, Include);
                if (HttpRuntime.Cache[cacheKey] != null)
                {
                    seller = filterContext.Controller.ViewBag.Seller = HttpRuntime.Cache[cacheKey];
                }
                else
                {
                    var sellerService = new SellersDBContext(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                    seller = sellerService.Get(string.Format("UrlName = '{0}'", subdomain)).FirstOrDefault();
                    if (!string.IsNullOrEmpty(Include))
                    {
                        foreach (var include in Include.Split(','))
                        {
                            PropertyInfo property = seller.GetType().GetProperty(include);
                            var method = typeof(BaseDomainModel).GetMethod("Include");
                            var generic = method.MakeGenericMethod(property.PropertyType.GetGenericArguments()[0]);
                            generic.Invoke(seller, new[] { include });
                        }
                    }
                    HttpRuntime.Cache.Insert(cacheKey, seller, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1));
                }
                filterContext.Controller.ViewBag.Seller = seller;
                filterContext.Controller.ViewBag.SellerUrl = subdomain;
            }
        }
    }
}