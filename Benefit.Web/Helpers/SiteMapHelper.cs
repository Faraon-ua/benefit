﻿using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace Benefit.Web.Helpers
{
    public class SiteMapHelper
    {
        private readonly string basePath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
        private const int MaxUrlSetNumber = 50000;

        public int Generate(UrlHelper urlHelper)
        {
            var siteMapCounter = 1;
            using (var db = new ApplicationDbContext())
            {
                var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
                var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
                var urlSet = new XElement(ns + "urlset");
                var count = 0;
                foreach (var page in db.InfoPages.Where(entry => entry.SellerId == null))
                {
                    var url = new XElement(ns + "url");
                    var loc = new XElement(ns + "loc", string.Concat(SettingsService.BaseHostName, urlHelper.RouteUrl("pagesRoute", new { id = page.UrlName })));
                    var lastmod = new XElement(ns + "lastmod", XmlConvert.ToString(page.LastModified));
                    url.Add(loc);
                    url.Add(lastmod);
                    urlSet.Add(url);
                    count++;
                }
                foreach (var seller in db.Sellers.Where(entry => entry.IsActive))
                {
                    var url = new XElement(ns + "url");
                    var loc = new XElement(ns + "loc",
                        string.Concat(
                            urlHelper.SubdomainAction(seller.UrlName, "Index", "Home", new { area = "" })
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

                const int ProductsPerPage = 10000;
                var productsPage = 0;
                List<Product> products = null;
                do
                {
                    products = db.Products.Include(entry => entry.Seller)
                        .Where(entry => entry.Seller.IsActive && entry.IsActive).OrderBy(entry => entry.SKU)
                        .Skip(productsPage * ProductsPerPage).Take(ProductsPerPage).ToList();

                    foreach (var product in products)
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
                        if (count == MaxUrlSetNumber)
                        {
                            doc.Add(urlSet);
                            using (var stream = File.Create(Path.Combine(basePath, string.Format("sitemap{0}.xml", siteMapCounter++))))
                            {
                                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                                {
                                    doc.Save(writer);
                                }
                            }
                            doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
                            urlSet = new XElement(ns + "urlset");
                            count = 0;
                        }
                    }
                    productsPage++;
                } while (products.Count == ProductsPerPage);
                doc.Add(urlSet);
                using (var stream = File.Create(Path.Combine(basePath, string.Format("sitemap{0}.xml", siteMapCounter))))
                {
                    using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                    {
                        doc.Save(writer);
                    }
                }

                //sitemap index
                doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
                var xmlns = XNamespace.Get("http://www.google.com/schemas/sitemap/0.84");
                var sitemapIndex = new XElement(xmlns + "sitemapindex");
                for (int i = 1; i <= siteMapCounter; i++)
                {
                    var sitemap = new XElement(xmlns + "sitemap");
                    var loc = new XElement(xmlns + "loc", string.Format("sitemap{0}.xml", i));
                    var lastmod = new XElement(xmlns + "lastmod", XmlConvert.ToString(DateTime.Now));
                    sitemap.Add(loc);
                    sitemap.Add(lastmod);
                    sitemapIndex.Add(sitemap);
                }
                doc.Add(sitemapIndex);
                using (var stream = File.Create(Path.Combine(basePath, "sitemap_index.xml")))
                {
                    using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                    {
                        doc.Save(writer);
                    }
                }
                return siteMapCounter;
            }
        }
    }
}