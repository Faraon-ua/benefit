using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Services.Admin;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Helpers;
using System.Data.Entity;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.SuperAdminRoleName)]
    public class AdminStuffController : AdminController
    {
        ApplicationDbContext db = new ApplicationDbContext();
        //todo: change controller name
        //
        // GET: /Admin/AdminStuff/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GenerateSitemap()
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
            var urlSet = new XElement(ns + "urlset");
            var count = 0;
            foreach (var page in db.InfoPages)
            {
                var url = new XElement(ns + "url");
                var loc = new XElement(ns + "loc", string.Concat(SettingsService.BaseHostName, Url.RouteUrl("pagesRoute", new { id = page.UrlName })));
                var lastmod = new XElement(ns + "lastmod", XmlConvert.ToString(page.LastModified));
                url.Add(loc);
                url.Add(lastmod);
                urlSet.Add(url);
                count++;
            }
            foreach (var seller in db.Sellers)
            {
                var url = new XElement(ns + "url");
                var loc = new XElement(ns + "loc",
                    string.Concat(
                        SettingsService.BaseHostName,
                        Url.RouteUrl(RouteConstants.SellersRouteName, new {id = seller.UrlName, action = string.Empty})
                        ));
                var lastmod = new XElement(ns + "lastmod", XmlConvert.ToString(seller.LastModified));
                url.Add(loc);
                url.Add(lastmod);
                urlSet.Add(url);
                count++;
            }
            foreach (var product in db.Products.Include(entry => entry.Category).Include(entry => entry.Seller).Where(entry => entry.Seller.IsActive && entry.IsActive))
            {
                var url = new XElement(ns + "url");
                var loc = new XElement(ns + "loc",
                    string.Concat(
                        SettingsService.BaseHostName,
                        Url.RouteUrl(RouteConstants.ProductRouteWithSellerName,
                            new
                            {
                                categoryUrl = product.Category.UrlName,
                                sellerUrl = product.Seller.UrlName,
                                productUrl = product.UrlName
                            })
                        ));
                var lastmod = new XElement(ns + "lastmod", XmlConvert.ToString(product.LastModified));
                url.Add(loc);
                url.Add(lastmod);
                urlSet.Add(url);
                count++;
            }
            doc.Add(urlSet);
            var basePath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            using (var stream = System.IO.File.Create(Path.Combine(basePath, "sitemap.xml")))
            {
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    doc.Save(writer);
                }
            }
            TempData["SuccessMessage"] = "Файл sitemap.xml згенеровано. Кількість лінків: " + count;
            return RedirectToAction("Index");
        }

        public ActionResult ClosePeriod()
        {
            var service = new ScheduleService();
            service.CloseQualificationPeriod();
            TempData["SuccessMessage"] = "Період було закрито";
            return View("Index");
        }

        public ActionResult BonusesCalculation()
        {
            var service = new ScheduleService();
            var result = service.ProcessBonuses();
            if (result == null) return View("BonusesCalculationError");
            var bonusesHtml = ControllerContext.RenderPartialToString("_BonusesCalculationPartial", result);
            var emailService = new EmailService();
            emailService.SendBonusesRozrahunokResults(bonusesHtml);
            return View(result);
        }

        public ActionResult ClearChat()
        {
            db.Messages.RemoveRange(db.Messages);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Чат було очищено";
            return View("Index");
        }
    }
}