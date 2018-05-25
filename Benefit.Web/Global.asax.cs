using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;

namespace Benefit.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer<ApplicationDbContext>(null);
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
            HttpCookie cookie = new HttpCookie(DomainConstants.SessionCookieName, Session.SessionID.ToString());
            cookie.Expires = DateTime.Now.AddYears(1);
            var host = Request.Url.Host;
            var index = host.IndexOf(".");
            if (index < 0)
            {
                return;
            }
            var subdomain = host.Substring(0, index);
            string[] blacklist = { "www", "benefit", "mail", "benefit-company", "uzhgorod" };
            if (!blacklist.Contains(subdomain))
            {
                host = host.Replace(subdomain + ".", string.Empty);
            }
            cookie.Domain = host;
            Response.SetCookie(cookie);
        }
    }
}
