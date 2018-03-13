
using System.Web.Mvc;
using Benefit.Web.Annotations;

namespace Benefit.Web.Helpers
{
    public static class UrlHelperExtension
    {
        public static string SubdomainAction(this UrlHelper urlHelper, string subDomain, string action, string controller = null, [CanBeNull]object routeValues = null)
        {
            var scheme = urlHelper.RequestContext.HttpContext.Request.Url.Scheme;
            var urlAction = urlHelper.Action(action, controller, routeValues, scheme);
            var hostPos = urlAction.IndexOf(urlHelper.RequestContext.HttpContext.Request.Url.Host);
            return urlAction.Insert(hostPos, subDomain + ".");
        }

        public static string SubdomainRoute(this UrlHelper urlHelper, string subDomain, string routeName, [CanBeNull] object routeValues)
        {
            var urlAction = urlHelper.RouteUrl(routeName, routeValues);
            return string.Format("{0}.{1}", subDomain, urlAction);
        }
    }
}