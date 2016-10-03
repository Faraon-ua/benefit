using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Benefit.Common.Constants;

namespace Benefit.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.QueryString.AllKeys.Contains(RouteConstants.ReferalUrlName))
            {
                var referalCookie = new HttpCookie(RouteConstants.ReferalCookieName,
                    HttpContext.Current.Request.QueryString[RouteConstants.ReferalUrlName]);
                HttpContext.Current.Response.Cookies.Add(referalCookie);
            }
        }
    }
}
