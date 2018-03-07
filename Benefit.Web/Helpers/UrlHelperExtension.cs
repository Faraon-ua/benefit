
using System.Web.Mvc;
using Benefit.Web.Annotations;

namespace Benefit.Web.Helpers
{
    public static class UrlHelperExtension
    {
        public static string SubdomainAction(this UrlHelper urlHelper, string subDomain, string action, [CanBeNull] string controller, [CanBeNull] object routeValues)
        {
            var urlAction = urlHelper.Action(action, controller, routeValues);
            return string.Format("{0}.{1}", subDomain, urlAction);
        }
    }
}