﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Benefit.Web.Filters
{
    public class SubdomainFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Url == null)
            {
                return;
            }
            var host = filterContext.HttpContext.Request.Url.Host;
            var index = host.IndexOf(".");
            if (index < 0)
            {
                return;
            }
            var subdomain = host.Substring(0, index);
            string[] blacklist = { "www", "benefit", "mail" };
            if (blacklist.Contains(subdomain))
            {
                return;
            }
            filterContext.RequestContext.RouteData.Values["subdomain"] = subdomain;
        }
    }
}