﻿using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace Benefit.Services.Domain
{
    public class ExportService
    {
        public void FetchOffers(List<XElement> offers, List<XElement> groups, Product product, List<ProductOption> variants, ProductOption currentVariant, string suffix, double priceChange, HttpRequest Request)
        {
            var group = new XElement("group", product.Name);
            group.Add(new XAttribute("id", groups.Count()));
            group.Add(new XAttribute("var_param_id", currentVariant.Id));
            foreach (var variant in currentVariant.ChildProductOptions)
            {
                var newSuffix = suffix + string.Format("{0} {1} ", currentVariant.Name, variant.Name);
                priceChange += variant.PriceGrowth;

                var prod = new XElement("offer", new XAttribute("id", product.Id + variant.Id));
                var available = product.IsActive
                           && (product.AvailabilityState != ProductAvailabilityState.NotInStock && product.AvailabilityState != ProductAvailabilityState.OnDemand)
                           && (product.AvailableAmount.GetValueOrDefault(0) > 0 || product.AvailabilityState == ProductAvailabilityState.AlwaysAvailable);
                prod.Add(new XAttribute("available", available));
                prod.Add(new XAttribute("group_id", groups.Count()));
                prod.Add(new XElement("name", string.Format("{0} {1}", product.Name, newSuffix)));
                prod.Add(new XElement("vendor", product.Vendor));
                prod.Add(new XElement("vendorCode", product.SKU));
                var price = product.Price;
                if (product.Currency != null)
                {
                    price *= product.Currency.Rate;
                }
                prod.Add(new XElement("price", price + priceChange));
                prod.Add(new XElement("currencyId", "UAH"));
                prod.Add(new XElement("stock_quantity", product.AvailableAmount ?? 100));
                if (product.OldPrice.HasValue)
                {
                    var oldprice = product.OldPrice;
                    if (product.Currency != null)
                    {
                        oldprice *= product.Currency.Rate;
                    }
                    prod.Add(new XElement("price_old", oldprice));
                }
                prod.Add(new XElement("url", string.Format("{0}://{1}/t/{2}-{3}", Request.Url.Scheme, Request.Url.Host, product.UrlName, product.SKU)));
                var categoryId = Math.Abs(product.CategoryId.GetHashCode());
                if (product.Category.MappedParentCategory != null)
                {
                    categoryId = Math.Abs(product.Category.MappedParentCategoryId.GetHashCode());
                }
                prod.Add(new XElement("categoryId", categoryId));
                foreach (var picture in product.Images.OrderBy(entry => entry.Order))
                {
                    var pictureUrl = picture.IsAbsoluteUrl
                        ? picture.ImageUrl
                        : string.Format("{0}://{1}/Images/ProductGallery/{2}/{3}", Request.Url.Scheme, Request.Url.Host, product.Id,
                            picture.ImageUrl);
                    prod.Add(new XElement("picture", pictureUrl));
                }

                prod.Add(new XElement("description", new XCData(product.Description)));
                prod.Add(new XElement("country_of_origin", product.OriginCountry));
                //add group param (variant)
                prod.Add(new XElement("param", new XAttribute("name", currentVariant.Name), new XAttribute("paramid", currentVariant.Id), variant.Name));
                foreach (var parameterProduct in product.ProductParameterProducts)
                {
                    prod.Add(new XElement("param", new XAttribute("name", parameterProduct.ProductParameter.Name), parameterProduct.StartText));
                }

                prod.Add(new XElement("param", new XAttribute("name", "Доставка"),
                    string.Join(",", product.Seller.ShippingMethods.Select(entry => entry.Name))));
                var paymentSB = new StringBuilder();
                if (product.Seller.IsCashPaymentActive)
                {
                    paymentSB.Append("Наличными");
                }

                if (product.Seller.IsAcquiringActive)
                {
                    paymentSB.Append("Картой Visa/MasterCard");
                }

                prod.Add(new XElement("param", new XAttribute("name", "Оплата"), paymentSB.ToString()));
                prod.Add(new XElement("param", new XAttribute("name", "Гарантия"), "Обмен/возврат товара в течение 14 дней"));

                offers.Add(prod);
                var currentVariantPosition = variants.IndexOf(currentVariant);
                if (variants.Count() > currentVariantPosition + 1)
                {
                    FetchOffers(offers, groups, product, variants, variants.ElementAt(currentVariantPosition + 1), newSuffix, priceChange, Request);
                }
            }
            groups.Add(group);
        }
        public string Export(string exportId, string savePath = null)
        {
            using (var db = new ApplicationDbContext())
            {
                var Request = HttpContext.Current.Request;
                var exportTask = db.ExportImports
                    .Include(entry => entry.ExportProducts)
                    .FirstOrDefault(entry => entry.Id == exportId);
                if (exportTask == null || !exportTask.IsActive)
                {
                    return null;
                }

                var productIds = exportTask.ExportProducts.Select(entry => entry.ProductId).ToList();
                var products = new List<Product>();
                //workaround for sql (can not process queries more than 2000 records for IN clause)
                var count = 0;
                var query = db.Products.AsNoTracking()
                    .Include(entry => entry.Seller.ShippingMethods)
                    .Include(entry => entry.ProductParameterProducts.Select(pp => pp.ProductParameter))
                    .Include(entry => entry.ProductOptions.Select(op => op.ChildProductOptions))
                    .Include(entry => entry.Images)
                    .Include(entry => entry.Currency)
                    .Include(entry => entry.Category.ExportCategories)
                    .Include(entry => entry.Category.SellerCategories)
                    .Include(entry => entry.Category.MappedParentCategory.ExportCategories)
                    .Include(entry => entry.Category.MappedParentCategory.SellerCategories);
                while (count < productIds.Count)
                {
                    var ids = productIds.OrderBy(id => id).Skip(count).Take(500).ToList();
                    products.AddRange(query.Where(entry => ids.Contains(entry.Id)));
                    count += 500;
                }

                #region Categories
                var dbcategories = products.Select(entry => entry.Category).ToList();
                for (var i = 0; i < dbcategories.Count; i++)
                {
                    if (dbcategories[i].MappedParentCategory != null)
                    {
                        foreach (var product in products.Where(entry => entry.CategoryId == dbcategories[i].Id))
                        {
                            product.CategoryId = dbcategories[i].MappedParentCategoryId;
                            product.Category = dbcategories[i].MappedParentCategory;
                        }
                        dbcategories[i] = dbcategories[i].MappedParentCategory;
                    }
                }
                dbcategories = dbcategories.Where(entry => entry != null).ToList();
                var exportCategories = new Dictionary<int, string>();
                foreach (var dbCat in dbcategories)
                {
                    var id = Math.Abs(dbCat.Id.GetHashCode());
                    if (!exportCategories.ContainsKey(id))
                    {
                        var catExport = dbCat.ExportCategories.FirstOrDefault(entry => entry.ExportId == exportId);
                        var catName = catExport == null ? "Не задано мапінг " + dbCat.Id : catExport.Name;
                        exportCategories.Add(id, catName);
                    }
                }
                var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
                var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
                var yml_catalog = new XElement(ns + "yml_catalog", new XAttribute("date", DateTime.Now.ToLocalDateTimeWithFormat()));
                XElement shop = null;
                if (exportTask.SyncType == SyncType.YmlExport)
                {
                    shop = new XElement("shop");
                    var domain = Request.Url.Host;
                    domain = domain.Substring(0, domain.IndexOf(".") > 0 ? domain.IndexOf(".") : domain.Length);
                    var seller = db.Sellers.FirstOrDefault(entry => entry.Domain == domain || entry.UrlName == domain);
                    var name = new XElement("name", "Интернет магазин " + (seller == null ? "Benefit-Company" : seller.Name));
                    var company = new XElement("company", seller == null ? "Benefit-Company" : seller.Name);
                    var url = new XElement("url", string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Host));
                    //var email = new XElement("email", "info.benefitcompany@gmail.com");
                    var currencies = new XElement("currencies");
                    var uah = new XElement("currency", new XAttribute("id", "UAH"), new XAttribute("rate", "1"));
                    currencies.Add(uah);
                    var categories = new XElement("categories");
                    foreach (var category in exportCategories)
                    {
                        var cat = new XElement("category") { Value = category.Value };
                        cat.Add(new XAttribute("id", category.Key));
                        categories.Add(cat);
                    }
                    shop.Add(name);
                    shop.Add(company);
                    shop.Add(url);
                    //shop.Add(email);
                    shop.Add(currencies);
                    shop.Add(categories);
                }
                else if (exportTask.SyncType == SyncType.YmlExportEpicentr)
                {
                    shop = yml_catalog;
                }

                #endregion

                var offers = new XElement("offers");
                List<XElement> offersList = new List<XElement>();
                List<XElement> groupsList = new List<XElement>();
                foreach (var product in products)
                {
                    var LocalizationService = new LocalizationService();
                    product.Localizations = LocalizationService.Get(product,
                                entry => entry.Name,
                                entry => entry.Description,
                                entry => entry.ShortDescription,
                                entry => entry.AltText,
                                entry => entry.Title);
                    var variantGroups = product.ProductOptions.Where(entry => entry.IsVariant).ToList();
                    if (variantGroups.Any() && exportTask.SyncType == SyncType.YmlExport)
                    {
                        FetchOffers(offersList, groupsList, product, variantGroups, variantGroups[0], string.Empty, 0, Request);
                    }
                    else
                    {
                        var prod = new XElement("offer", new XAttribute("id", product.Id));
                        var available = product.IsActive
                            && (product.AvailabilityState != ProductAvailabilityState.NotInStock && product.AvailabilityState != ProductAvailabilityState.OnDemand)
                            && (product.AvailableAmount.GetValueOrDefault(0) > 0 || product.AvailabilityState == ProductAvailabilityState.AlwaysAvailable);
                        prod.Add(new XAttribute("available", available));
                        var name = new XElement("name", product.Name);
                        prod.Add(name);
                        var descr = new XElement("description", new XCData(product.Description));
                        prod.Add(descr);
                        if (exportTask.SyncType == SyncType.YmlExportEpicentr)
                        {
                            name.Add(new XAttribute("lang", "ua"));
                            descr.Add(new XAttribute("lang", "ua"));
                            if (product.Localizations.Any())
                            {
                                var ruNameLoc = product.Localizations.FirstOrDefault(entry => entry.ResourceField == "Name");
                                var ruDescrLoc = product.Localizations.FirstOrDefault(entry => entry.ResourceField == "Description");
                                if (ruNameLoc != null && !string.IsNullOrEmpty(ruNameLoc.ResourceValue))
                                {
                                    var ruName = new XElement("name", product.Localizations.FirstOrDefault(entry => entry.ResourceField == "Name").ResourceValue);
                                    ruName.Add(new XAttribute("lang", "ru"));
                                    prod.Add(ruName);
                                }
                                if (ruDescrLoc != null && !string.IsNullOrEmpty(ruDescrLoc.ResourceValue))
                                {
                                    var ruDescr = new XElement("name", new XCData(product.Localizations.FirstOrDefault(entry => entry.ResourceField == "Description").ResourceValue));
                                    ruDescr.Add(new XAttribute("lang", "ru"));
                                    prod.Add(ruDescr);
                                }
                            }
                        }
                        prod.Add(new XElement("vendor", product.Vendor));
                        prod.Add(new XElement("vendorCode", product.SKU));
                        if (product.Currency != null)
                        {
                            product.Price *= product.Currency.Rate;
                        }
                        var sellerCategory = product.Category.SellerCategories.FirstOrDefault(sc => sc.CategoryId == product.CategoryId && sc.SellerId == product.SellerId);
                        if (sellerCategory != null && sellerCategory.CustomMargin.HasValue)
                        {
                            if (product.OldPrice.HasValue)
                            {
                                product.OldPrice += product.OldPrice * sellerCategory.CustomMargin.Value / 100;
                            }
                            product.Price += product.Price * sellerCategory.CustomMargin.Value / 100;
                        }
                        if (product.OldPrice.HasValue)
                        {
                            if (product.Currency != null)
                            {
                                product.OldPrice *= product.Currency.Rate;
                            }
                            prod.Add(new XElement("price_old", product.OldPrice));
                        }
                        if (product.PromoPrice.HasValue)
                        {
                            if (product.Currency != null)
                            {
                                product.PromoPrice *= product.Currency.Rate;
                            }
                            prod.Add(new XElement("price_promo", product.PromoPrice));
                        }
                        prod.Add(new XElement("price", product.Price));
                        prod.Add(new XElement("currencyId", "UAH"));
                        prod.Add(new XElement("stock_quantity", product.AvailableAmount ?? 100));

                        prod.Add(new XElement("url", string.Format("{0}://{1}/t/{2}-{3}", Request.Url.Scheme, Request.Url.Host, product.UrlName, product.SKU)));
                        var categoryId = Math.Abs(product.CategoryId.GetHashCode());
                        if (product.Category.MappedParentCategory != null)
                        {
                            categoryId = Math.Abs(product.Category.MappedParentCategoryId.GetHashCode());
                        }
                        if (exportTask.SyncType == SyncType.YmlExport)
                        {
                            prod.Add(new XElement("categoryId", categoryId));
                        }
                        else if (exportTask.SyncType == SyncType.YmlExportEpicentr)
                        {
                            var cat = exportCategories.FirstOrDefault(entry => entry.Key == categoryId);
                            prod.Add(new XElement("category", cat.Value));
                        }
                        foreach (var picture in product.Images.Where(entry => entry.ImageType == ImageType.ProductGallery).OrderBy(entry => entry.Order))
                        {
                            var pictureUrl = picture.IsAbsoluteUrl
                                ? picture.ImageUrl
                                : string.Format("{0}://{1}/Images/ProductGallery/{2}/{3}", Request.Url.Scheme, Request.Url.Host, product.Id,
                                    picture.ImageUrl);
                            prod.Add(new XElement("picture", pictureUrl));
                        }
                        prod.Add(new XElement("country_of_origin", product.OriginCountry));
                        foreach (var parameterProduct in product.ProductParameterProducts)
                        {
                            prod.Add(new XElement("param", new XAttribute("name", parameterProduct.ProductParameter.Name), parameterProduct.StartText));
                        }

                        prod.Add(new XElement("param", new XAttribute("name", "Доставка"),
                            string.Join(",", product.Seller.ShippingMethods.Select(entry => entry.Name))));
                        var paymentSB = new StringBuilder();
                        if (product.Seller.IsCashPaymentActive)
                        {
                            paymentSB.Append("Наличными");
                        }
                        if (product.Seller.IsAcquiringActive)
                        {
                            if (paymentSB.Length > 0)
                            {
                                paymentSB.Append(", ");
                            }
                            paymentSB.Append("Картой Visa/MasterCard");
                        }

                        prod.Add(new XElement("param", new XAttribute("name", "Оплата"), paymentSB.ToString()));
                        prod.Add(new XElement("param", new XAttribute("name", "Гарантия"), "Обмен/возврат товара в течение 14 дней"));

                        offersList.Add(prod);
                    }
                }
                foreach (var o in offersList)
                {
                    offers.Add(o);
                }

                if (groupsList.Any())
                {
                    var groups = new XElement("groups");
                    foreach (var g in groupsList)
                    {
                        groups.Add(g);
                    }
                    shop.Add(groups);
                }
                shop.Add(offers);
                if (exportTask.SyncType == SyncType.YmlExport)
                {
                    yml_catalog.Add(shop);
                    doc.Add(yml_catalog);
                }
                else if (exportTask.SyncType == SyncType.YmlExportEpicentr)
                {
                    doc.Add(shop);
                }
                if (savePath != null)
                {
                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath);
                    }
                    doc.Save(savePath);
                }
                return doc.ToString();
            }
        }
    }
}
