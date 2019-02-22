using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Web.Helpers;
using System.Web.Mvc;

namespace Benefit.Web.Controllers
{
    public class RobotsController : Controller
    {
        public virtual ActionResult Index()
        {
            string robotsResult = @"User-agent: *
Disallow: /pages/benefit_tv
Disallow: /pages/faq
Disallow: /search
Disallow: /cart
Disallow: /account";
            var sitemapFileName = "sitemap.xml";
            if (Request.Url.Host == SettingsService.BaseHostName || Request.Url.Host == "localhost")
            {
                sitemapFileName = "sitemap_index.xml";
            }
            robotsResult += string.Format("\nSitemap: {0}://{1}/{2}", Request.Url.Scheme, Request.Url.Host, sitemapFileName);
            return Content(robotsResult, "text/plain");
        }

        public ActionResult Sitemap()
        {
            var siteMapHelper = new SiteMapHelper();
            var content = siteMapHelper.Generate(Url, Request.Url.Scheme + "://" + Request.Url.Host, false, (ViewBag.Seller as Seller));
            return Content(content, "text/xml");
        }
    }
}