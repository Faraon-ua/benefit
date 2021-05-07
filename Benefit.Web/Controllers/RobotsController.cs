using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Web.Helpers;
using System.Web.Mvc;
using Benefit.Web.Filters;

namespace Benefit.Web.Controllers
{
    public class RobotsController : Controller
    {
        [FetchSeller]
        public virtual ActionResult Index()
        {
            string robotsResult = @"User-agent: *
Disallow: /pages/benefit_tv
Disallow: /pages/faq
Disallow: /search
Disallow: /cart
Disallow: /account";
            var sitemapFileName = "sitemap.xml";
            var seller = (ViewBag.Seller as Seller);
            if (seller == null)
            {
                sitemapFileName = "sitemap_index.xml";
            }
            robotsResult += string.Format("\nSitemap: {0}://{1}/{2}", Request.Url.Scheme, Request.Url.Host, sitemapFileName);
            return Content(robotsResult, "text/plain");
        }

        //only for sellers, for benefit.ua sitemap_index is used
        [FetchSeller]
        public ActionResult Sitemap()
        {
            var siteMapHelper = new SiteMapHelper();
            var content = siteMapHelper.Generate(Url, Request.Url.Scheme + "://" + Request.Url.Host, false, (ViewBag.Seller as Seller));
            return Content(content, "text/xml");
        }
    }
}