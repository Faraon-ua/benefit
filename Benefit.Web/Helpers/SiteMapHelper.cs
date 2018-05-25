using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Services;

namespace Benefit.Web.Helpers
{
    public class SiteMapHelper
    {
        public int Generate(UrlHelper urlHelper)
        {
            using (var db = new ApplicationDbContext())
            {
                var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
                var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
                var urlSet = new XElement(ns + "urlset");
                var count = 0;
                foreach (var page in db.InfoPages.Where(entry=>entry.SellerId == null))
                {
                    var url = new XElement(ns + "url");
                    var loc = new XElement(ns + "loc", string.Concat(SettingsService.BaseHostName, urlHelper.RouteUrl("pagesRoute", new { id = page.UrlName })));
                    var lastmod = new XElement(ns + "lastmod", XmlConvert.ToString(page.LastModified));
                    url.Add(loc);
                    url.Add(lastmod);
                    urlSet.Add(url);
                    count++;
                }
                foreach (var seller in db.Sellers.Where(entry=>entry.IsActive))
                {
                    var url = new XElement(ns + "url");
                    var loc = new XElement(ns + "loc",
                        string.Concat(
                            urlHelper.SubdomainAction(seller.UrlName, "Index", "Home", new { area = ""})
                            ));
                    var lastmod = new XElement(ns + "lastmod", XmlConvert.ToString(seller.LastModified));
                    url.Add(loc);
                    url.Add(lastmod);
                    urlSet.Add(url);
                    count++;
                }
                foreach (var cat in db.Categories.Where(entry => !entry.IsSellerCategory && entry.IsActive).ToList())
                {
                    var url = new XElement(ns + "url");
                    var loc = new XElement(ns + "loc",
                        string.Concat(
                            SettingsService.BaseHostName,
                            urlHelper.RouteUrl(RouteConstants.CatalogRouteName,
                                new
                                {
                                    categoryUrl = cat.UrlName
                                })
                            ));
                    var lastmod = new XElement(ns + "lastmod", XmlConvert.ToString(cat.LastModified));
                    url.Add(loc);
                    url.Add(lastmod);
                    urlSet.Add(url);
                    count++;
                }
                foreach (var product in db.Products.Include(entry => entry.Seller).Where(entry => entry.Seller.IsActive && entry.IsActive))
                {
                    var url = new XElement(ns + "url");
                    var loc = new XElement(ns + "loc",
                        string.Concat(
                            SettingsService.BaseHostName,
                            urlHelper.RouteUrl(RouteConstants.ProductRouteName, new { productUrl = string.Format("{0}-{1}", product.UrlName, product.SKU) })
                        ));
                    var lastmod = new XElement(ns + "lastmod", XmlConvert.ToString(product.LastModified));
                    url.Add(loc);
                    url.Add(lastmod);
                    urlSet.Add(url);
                    count++;
                }
                doc.Add(urlSet);
                var basePath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                using (var stream = File.Create(Path.Combine(basePath, "sitemap.xml")))
                {
                    using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                    {
                        doc.Save(writer);
                    }
                }
                return count;
            }
        }
    }
}