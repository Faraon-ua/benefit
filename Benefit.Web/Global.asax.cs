﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Web.App_Start;
using Benefit.Web.Controllers;

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
            AutomapperConfig.Init();
        }
        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            if (custom.ToLowerInvariant() == "ismobile" && context.Request.Browser.IsMobileDevice)
            {
                return "mobile";
            }
            return base.GetVaryByCustomString(context, custom);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (RouteConstants.Redirections.ContainsKey(Request.Url.Host))
            {
                Response.StatusCode = 301;
                var location =
                    Request.Url.AbsoluteUri.Replace(Request.Url.Host, RouteConstants.Redirections[Request.Url.Host]);
                Response.AddHeader("Location", location);
                Response.End();
            }
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

        protected void Application_EndRequest()
        {
            if (Context.Response.StatusCode == 404 || Context.Response.StatusCode == 503)
            {
                Response.Clear();

                var rd = new RouteData();
                rd.Values["controller"] = "Error";
                rd.Values["action"] = "NotFound";

                IController c = new ErrorController();
                c.Execute(new RequestContext(new HttpContextWrapper(Context), rd));
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            HttpContext.Current.Session.Add("__MyAppSession", string.Empty);
            HttpCookie cookie = new HttpCookie(DomainConstants.SessionCookieName, Session.SessionID);
            cookie.Expires = DateTime.Now.AddYears(1);
            var host = Request.Url.Host;
            var index = host.IndexOf(".");
            if (index < 0)
            {
                return;
            }
            var subdomain = host.Substring(0, index);
            var blacklist = new List<string> { "www", "benefit", "mail", "benefit-company", "dechevle" };

            if (!blacklist.Contains(subdomain) && host.Contains(SettingsService.BaseHostName))
            {
                host = host.Replace(subdomain + ".", string.Empty);
            }
            cookie.Domain = host;
            Response.SetCookie(cookie);
        }
    }
}
