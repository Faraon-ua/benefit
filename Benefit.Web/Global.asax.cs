using System;
using System.Linq;
using System.Web;
using System.Web.Http;
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
            GlobalConfiguration.Configure(WebApiConfig.Register);
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
            if ((Request.Url.Host.StartsWith("www") || !Request.IsSecureConnection) && !Request.Url.IsLoopback)
            {
                var builder = new UriBuilder(Request.Url);
                if (Request.Url.Host.StartsWith("www"))
                {
                    builder.Host = Request.Url.Host.Replace("www.", "");
                }
                builder.Port = 443;
                builder.Scheme = "https";
                Response.StatusCode = 301;
                Response.AddHeader("Location", builder.ToString());
                Response.End();
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            HttpContext.Current.Session.Add("__MyAppSession", string.Empty);
        }
    }
}
