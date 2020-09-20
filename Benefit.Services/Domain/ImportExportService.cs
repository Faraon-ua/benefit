using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Excel;
using Benefit.Services.Import;
using Benefit.Web.Helpers;
using Benefit.Web.Models.Admin;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Benefit.Services.Domain
{
    public class ImportExportService
    {
        private SellerService SellerService = new SellerService();
        private CategoriesService categoriesService = new CategoriesService();
        private ProductsService productsService = new ProductsService();
        private ImagesService ImagesService = new ImagesService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
        object lockObj = new object();

        #region Export

        public void FetchOffers(List<XElement> offers, Product product, List<ProductOption> variants, ProductOption currentVariant, string suffix, double priceChange, HttpRequest Request)
        {
            foreach (var variant in currentVariant.ChildProductOptions)
            {
                var newSuffix = suffix + string.Format("{0} {1} ", currentVariant.Name, variant.Name);
                priceChange += variant.PriceGrowth;

                var prod = new XElement("offer", new XAttribute("id", product.Id));
                var available = product.IsActive && product.AvailabilityState != ProductAvailabilityState.NotInStock;
                prod.Add(new XAttribute("available", available));
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

                prod.Add(new XElement("description", product.Description));
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
                    paymentSB.Append("Картой Visa/MasterCard");
                }

                prod.Add(new XElement("param", new XAttribute("name", "Оплата"), paymentSB.ToString()));
                prod.Add(new XElement("param", new XAttribute("name", "Гарантия"), "Обмен/возврат товара в течение 14 дней"));

                offers.Add(prod);

                var currentVariantPosition = variants.IndexOf(currentVariant);
                if (variants.Count() > currentVariantPosition + 1)
                {
                    FetchOffers(offers, product, variants, variants.ElementAt(currentVariantPosition + 1), newSuffix, priceChange, Request);
                }
            }
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
                var products = db.Products.AsNoTracking()
                    .Include(entry => entry.Seller.ShippingMethods)
                    .Include(entry => entry.ProductParameterProducts.Select(pp => pp.ProductParameter))
                    .Include(entry => entry.ProductOptions.Select(op => op.ChildProductOptions))
                    .Include(entry => entry.Images)
                    .Include(entry => entry.Currency)
                    .Include(entry => entry.Category.ExportCategories)
                    .Include(entry => entry.Category.SellerCategories)
                    .Include(entry => entry.Category.MappedParentCategory.ExportCategories)
                    .Include(entry => entry.Category.MappedParentCategory.SellerCategories)
                    .Where(entry => productIds.Contains(entry.Id)).ToList();

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
                var shop = new XElement("shop");
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
                #endregion

                var offers = new XElement("offers");
                List<XElement> offersList = new List<XElement>();
                foreach (var product in products)
                {
                    var variantGroups = product.ProductOptions.Where(entry => entry.IsVariant).ToList();
                    if (variantGroups.Any())
                    {
                        FetchOffers(offersList, product, variantGroups, variantGroups[0], string.Empty, 0, Request);
                    }
                    else
                    {
                        var prod = new XElement("offer", new XAttribute("id", product.Id));
                        var available = product.IsActive && product.AvailabilityState != ProductAvailabilityState.NotInStock;
                        prod.Add(new XAttribute("available", available));
                        prod.Add(new XElement("name", product.Name));
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

                        offersList.Add(prod);
                    }
                }
                foreach (var o in offersList)
                {
                    offers.Add(o);
                }

                shop.Add(name);
                shop.Add(company);
                shop.Add(url);
                //shop.Add(email);
                shop.Add(currencies);
                shop.Add(categories);
                shop.Add(offers);
                yml_catalog.Add(shop);
                doc.Add(yml_catalog);
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

        #endregion

        #region 1C

        public bool ImportFrom1C(XDocument xml, Seller seller)
        {
            _logger.Warn(string.Format("1c import started for {0} at {1}", seller.Name, DateTime.UtcNow.ToLocalDateTimeWithFormat()));
            try
            {
                var rawXmlCategories = xml.Descendants("Группы").First().Elements().ToList();
                var resultXmlCategories = GetAllFiniteCategories(rawXmlCategories);
                var resultXmlCategoryIds = resultXmlCategories.Select(entry => entry.Element("Ид").Value);
                using (var db = new ApplicationDbContext())
                {
                    CreateAndUpdate1CCategories(resultXmlCategories, seller.Id, db);
                    db.SaveChanges();
                    DeleteImportCategories(seller, resultXmlCategories, SyncType.OneCCommerceMl, db);
                    db.SaveChanges();

                    var xmlProducts = xml.Descendants("Товары").First().Elements()
                        .Where(entry => entry.Element("Группы") != null)
                        .Where(entry => resultXmlCategoryIds.Contains(entry.Element("Группы").Element("Ид").Value)).ToList();
                    var ids = xmlProducts.Select(entry => entry.Element("Ид").Value).ToList();
                    var xmlProductsSkipped = xml.Descendants("Товары").First().Elements()
                        .Where(entry => !ids.Contains(entry.Element("Ид").Value)).ToList();
                    AddAndUpdate1СProducts(xmlProducts, seller.Id, seller.UrlName, db);
                    db.SaveChanges();
                    DeletePromUaProducts(xmlProducts, seller.Id, SyncType.OneCCommerceMl, db);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
            return true;
        }

        private void AddAndUpdate1СProducts(List<XElement> xmlProducts, string sellerId, string sellerUrl, ApplicationDbContext db)
        {
            var maxSku = db.Products.Max(entry => entry.SKU) + 1;
            var xmlProductIds = xmlProducts.Select(entry => entry.Element("Ид").Value).ToList();
            var dbProducts = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported).ToList();
            var dbProductIds = dbProducts.Select(entry => entry.Id).ToList();
            var productIdsToAdd = xmlProductIds.Where(entry => !dbProductIds.Contains(entry)).ToList();
            //additional check all over DB
            productIdsToAdd = productIdsToAdd.Where(entry => !db.Products.Select(pr => pr.Id).Contains(entry)).ToList();
            var productIdsToUpdate = xmlProductIds.Where(dbProductIds.Contains).ToList();

            var productsToAddList = new List<Product>();
            var imagesToAddList = new List<Image>();

            var defaultImageIds = dbProducts.Select(entry => entry.DefaultImageId).ToList();
            var existingImages = db.Images.Where(entry => dbProductIds.Contains(entry.ProductId) && entry.IsImported && !defaultImageIds.Contains(entry.Id)).ToList();
            db.DeleteWhereColumnIn(existingImages);

            Parallel.ForEach(productIdsToAdd, (productIdToAdd) =>
            {
                var xmlProduct = xmlProducts.First(entry => entry.Element("Ид").Value == productIdToAdd);
                var name = HttpUtility
                    .HtmlDecode(xmlProduct.Element("Наименование").Value.Replace("\n", "").Replace("\r", "").Trim())
                    .Truncate(256);
                var descr = xmlProduct.Element("Описание").GetValueOrDefault(string.Empty).Replace("\n", "<br/>");
                var urlName = name.Translit().Truncate(128);
                var product = new Product()
                {
                    Id = xmlProduct.Element("Ид").Value,
                    ExternalId = xmlProduct.Element("ШтрихКод").GetValueOrDefault(null),
                    Name = name,
                    UrlName = urlName,
                    CategoryId = xmlProduct.Element("Группы").Element("Ид").Value,
                    SellerId = sellerId,
                    Description = string.IsNullOrEmpty(descr) ? name : descr,
                    AvailabilityState = ProductAvailabilityState.AlwaysAvailable,
                    IsActive = true,
                    IsImported = true,
                    DoesCountForShipping = true,
                    AddedOn = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    AltText = name.Truncate(100),
                    ShortDescription = name
                };

                lock (lockObj)
                {
                    productsToAddList.Add(product);
                }

                var order = 0;
                lock (lockObj)
                {
                    imagesToAddList.AddRange(xmlProduct.Elements("Картинка").Where(entry => !string.IsNullOrEmpty(entry.Value)).Select(xmlImage => new Image()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ImageType = ImageType.ProductGallery,
                        ImageUrl = new Uri(SettingsService.BaseHostName).Append("FTP").Append("LocalUser").Append(sellerUrl).Append(xmlImage.Value).AbsoluteUri,
                        IsAbsoluteUrl = true,
                        Order = order++,
                        ProductId = product.Id,
                        IsImported = true
                    }));
                }
            });

            Parallel.ForEach(productIdsToUpdate, (productIdToUpdate) =>
            {
                var product = dbProducts.FirstOrDefault(entry => entry.Id == productIdToUpdate);
                var xmlProduct = xmlProducts.First(entry => entry.Element("Ид").Value == productIdToUpdate);

                product.ExternalId = xmlProduct.Element("ШтрихКод").GetValueOrDefault(null);
                product.IsImported = true;
                product.IsActive = true;
                product.CategoryId = xmlProduct.Element("Группы").Element("Ид").Value;
                product.AvailabilityState = ProductAvailabilityState.AlwaysAvailable;
                product.LastModified = DateTime.UtcNow;

                var order = 0;
                lock (lockObj)
                {
                    imagesToAddList.AddRange(xmlProduct.Elements("Картинка").Where(entry => !string.IsNullOrEmpty(entry.Value)).Select(xmlImage => new Image()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ImageType = ImageType.ProductGallery,
                        ImageUrl = new Uri(SettingsService.BaseHostName).Append("FTP").Append("LocalUser").Append(sellerUrl).Append(xmlImage.Value).AbsoluteUri,
                        IsAbsoluteUrl = true,
                        Order = order++,
                        ProductId = product.Id,
                        IsImported = true
                    }));
                }
            });
            foreach (var product in productsToAddList)
            {
                product.SKU = maxSku;
                product.UrlName = product.UrlName.Insert(0, maxSku++ + "-").Truncate(128);
            }

            db.InsertIntoMembers(productsToAddList);
            db.SaveChanges();
            db.InsertIntoMembers(imagesToAddList);
            foreach (var image in imagesToAddList)
            {
                var uri = new Uri(image.ImageUrl);
                var path = originalDirectory + uri.LocalPath;
                ImagesService.ResizeToSiteRatio(path, ImageType.ProductGallery);
            }
        }

        private List<XElement> GetAllFiniteCategories(IEnumerable<XElement> xmlCategories)
        {
            var resultXmlCategories = new List<XElement>();
            var hadChildren = false;
            foreach (var rawXmlCategory in xmlCategories)
            {
                if (rawXmlCategory.Element("Группы") != null)
                {
                    resultXmlCategories.AddRange(rawXmlCategory.Element("Группы").Elements());
                    hadChildren = true;
                }
                else
                {
                    resultXmlCategories.Add(rawXmlCategory);
                }
            }
            if (hadChildren)
            {
                resultXmlCategories = GetAllFiniteCategories(resultXmlCategories);
            }
            return resultXmlCategories;
        }

        private void CreateAndUpdate1CCategories(List<XElement> xmlCategories, string sellerId, ApplicationDbContext db)
        {
            var hasNewContent = false;
            var sellerCats = new List<SellerCategory>();
            var seller = db.Sellers.Include(entry => entry.SellerCategories)
                .FirstOrDefault(entry => entry.Id == sellerId);
            var sellerCatsDiscount = seller.SellerCategories.Where(entry => entry.CustomDiscount.HasValue)
                .ToDictionary(entry => entry.CategoryId, entry => entry.CustomDiscount.Value);
            db.SellerCategories.RemoveRange(seller.SellerCategories.Where(entry => !entry.IsDefault));
            foreach (var xmlCategory in xmlCategories)
            {
                var catId = xmlCategory.Element("Ид").Value;
                var catName = xmlCategory.Element("Наименование").Value.Replace("\n", "").Replace("\r", "").Trim();
                var dbCategory = db.Categories.FirstOrDefault(entry => entry.Id == catId);
                if (dbCategory == null)
                {
                    if (!hasNewContent)
                    {
                        hasNewContent = true;
                    }

                    dbCategory = new Category()
                    {
                        Id = catId,
                        IsSellerCategory = true,
                        SellerId = sellerId,
                        Name = catName.Truncate(64),
                        UrlName = string.Format("{0}-{1}", catId, catName.Translit()).Truncate(128),
                        ExternalIds = catId,
                        IsActive = true,
                        LastModified = DateTime.UtcNow,
                        LastModifiedBy = "ImportFrom1C"
                    };
                    db.Categories.Add(dbCategory);
                }
                else
                {
                    sellerCats.Add(new SellerCategory()
                    {
                        CategoryId = dbCategory.MappedParentCategoryId,
                        SellerId = sellerId
                    });
                    dbCategory.ExternalIds = catId;
                    dbCategory.IsActive = true;
                    dbCategory.Name = catName.Truncate(64);
                    dbCategory.UrlName = string.Format("{0}-{1}", catId, catName.Translit()).Truncate(128);
                    db.Entry(dbCategory).State = EntityState.Modified;
                }
            }

            sellerCats = sellerCats.Where(entry => entry.CategoryId != null).Distinct(new SellerCategoryComparer()).ToList();
            foreach (var d in sellerCatsDiscount)
            {
                var sellerCat = sellerCats.FirstOrDefault(entry => entry.CategoryId == d.Key);
                if (sellerCat != null)
                {
                    sellerCat.CustomDiscount = d.Value;
                }
            }
            db.SellerCategories.AddRange(sellerCats);
            if (hasNewContent)
            {
                var importTask = db.ExportImports.FirstOrDefault(entry => entry.SellerId == sellerId);
                importTask.HasNewContent = true;
            }
        }

        #endregion

        #region PromUa

        private void CreateAndUpdateYmlCategories(List<XElement> xmlCategories, string sellerUrlName,
            string sellerId, ApplicationDbContext db, Category parent = null)
        {
            var hasNewContent = false;
            List<XElement> xmlCats = null;
            if (parent == null)
            {
                xmlCats = xmlCategories.Where(entry => entry.Attribute("parentId") == null || (entry.Attribute("parentId") != null && entry.Attribute("parentId").Value == "0")).ToList();
            }
            else
            {
                xmlCats = xmlCategories.Where(entry =>
                    entry.Attribute("parentId") != null && entry.Attribute("parentId").Value == parent.ExternalIds).ToList();
            }

            foreach (var xmlCategory in xmlCats)
            {
                var catId = xmlCategory.Attribute("id").Value;
                var catName = xmlCategory.Value.Replace("\n", "").Replace("\r", "").Trim();
                var dbCategory =
                    db.Categories.FirstOrDefault(entry => entry.ExternalIds == catId && entry.SellerId == sellerId) ??
                    db.Categories.FirstOrDefault(entry => entry.Id == catId && entry.SellerId == sellerId);
                if (dbCategory == null)
                {
                    if (!hasNewContent)
                    {
                        hasNewContent = true;
                    }
                    dbCategory = new Category()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ExternalIds = catId,
                        ParentCategoryId = parent == null ? null : parent.Id,
                        IsSellerCategory = true,
                        SellerId = sellerId,
                        Name = catName.Truncate(64),
                        UrlName = string.Format("{0}-{1}-{2}", sellerId, catId, catName.Translit()).Truncate(128),
                        Description = catName,
                        MetaDescription = catName,
                        IsActive = true,
                        LastModified = DateTime.UtcNow,
                        LastModifiedBy = "ImportFromYml"
                    };
                    db.Categories.Add(dbCategory);
                }
                else
                {
                    dbCategory.IsActive = true;
                    dbCategory.ExternalIds = catId;
                    dbCategory.ParentCategoryId = parent == null ? null : parent.Id;
                    dbCategory.Name = catName.Truncate(64);
                    dbCategory.UrlName =
                        string.Format("{0}-{1}-{2}", sellerId, catId, catName.Translit()).Truncate(128);
                    db.Entry(dbCategory).State = EntityState.Modified;
                }

                CreateAndUpdateYmlCategories(xmlCategories, sellerUrlName, sellerId, db, dbCategory);
            }
            if (hasNewContent)
            {
                var importTask = db.ExportImports.FirstOrDefault(entry => entry.SellerId == sellerId);
                importTask.HasNewContent = true;
            }
        }

        public void DeleteImportCategories(Seller seller, IEnumerable<XElement> xmlCategories, SyncType importType, ApplicationDbContext db)
        {
            var currentSellercategoyIds = seller.MappedCategories.Select(entry => entry.ExternalIds).Distinct().ToList();
            List<string> xmlCategoryIds = null;
            if (importType == SyncType.OneCCommerceMl)
            {
                xmlCategoryIds = xmlCategories.Select(entry => entry.Element("Ид").Value).ToList();
            }
            if (importType == SyncType.Yml)
            {
                xmlCategoryIds = xmlCategories.Select(entry => entry.Attribute("id").Value).ToList();
            }
            if (importType == SyncType.Gbs)
            {
                xmlCategoryIds = xmlCategories.Select(entry => entry.Element("Id").Value).ToList();
            }
            var catIdsToRemove = currentSellercategoyIds.Except(xmlCategoryIds).ToList();
            foreach (var catId in catIdsToRemove)
            {
                var dbCategories = db.Categories.Where(entry => entry.SellerId == seller.Id && entry.ExternalIds == catId).ToList();
                dbCategories.ForEach(entry =>
                {
                    entry.IsActive = false;
                    db.Entry(entry).State = EntityState.Modified;
                });
            }
        }

        private void AddAndUpdateYmlProducts(List<XElement> xmlProducts, string sellerId, IEnumerable<string> categoryIds)
        {
            using (var db = new ApplicationDbContext())
            {
                var maxSku = db.Products.Max(entry => entry.SKU) + 1;
                var xmlProductIds = xmlProducts.Select(entry => entry.Attribute("id").Value).ToList();
                var dbProducts = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported).ToList();
                var dbProductIds = dbProducts.Select(entry => entry.Id).ToList();
                var productIdsToAdd = xmlProductIds.Where(entry => !dbProductIds.Contains(entry)).ToList();
                var xmlProductsToAdd = xmlProducts.Where(entry => productIdsToAdd.Contains(entry.Attribute("id").Value)).ToList();
                var productIdsToUpdate = xmlProductIds.Where(dbProductIds.Contains).ToList();
                var xmlCategoryIds = xmlProducts.Select(pr => pr.Element("categoryId").Value).Distinct().ToList();
                var categories = db.Categories
                    .Where(entry => xmlCategoryIds.Contains(entry.ExternalIds) && entry.SellerId == sellerId).ToList();
                var productsToAddList = new List<Product>();
                var imagesToAddList = new List<Image>();

                //var existingImages = db.Images.AsNoTracking().Where(entry => dbProductIds.Contains(entry.ProductId)).ToList();
                var currencies = db.Currencies
                    .Where(entry => entry.SellerId == null || entry.SellerId == sellerId)
                    .OrderBy(entry => entry.Provider)
                    .ToList();

                //parameters
                var productsGroupByCategoryId = xmlProductsToAdd.GroupBy(entry => entry.Element("categoryId").Value)
                    .Where(entry => categoryIds.Contains(entry.Key)).ToList();

                var categryIds = categories.Select(pr => pr.Id).ToList();
                //var dbProductParameters =
                //    db.ProductParameters.AsNoTracking().Where(
                //        entry => categryIds.Contains(entry.CategoryId)).ToList();
                //var productParameterIds = dbProductParameters.Select(pr => pr.Id).ToList();
                //var dbProductParameterValues =
                //    db.ProductParameterValues.AsNoTracking().Where(
                //        entry => productParameterIds.Contains(entry.ProductParameterId)).ToList();
                //var dbProductParameterProducts = db.ProductParameterProducts.AsNoTracking().Where(
                //    entry => productParameterIds.Contains(entry.ProductParameterId)).ToList();

                //db.DeleteWhereColumnIn(existingImages);
                //existingImages.Clear();
                //db.DeleteWhereColumnIn(dbProductParameterProducts, "ProductParameterId");
                //dbProductParameterProducts.Clear();
                //db.DeleteWhereColumnIn(dbProductParameterValues);
                //dbProductParameterValues.Clear();
                //db.DeleteWhereColumnIn(dbProductParameters);
                //dbProductParameters.Clear();

                var dbProductParameterProducts = new List<ProductParameterProduct>();
                var dbProductParameterValues = new List<ProductParameterValue>();
                var dbProductParameters = new List<ProductParameter>();

                foreach (var categoryGroupParams in productsGroupByCategoryId)
                {
                    var xmlParameters =
                        categoryGroupParams.SelectMany(entry => entry.Elements("param"))
                            .Select(
                                entry =>
                                    new
                                    {
                                        Name = entry.Attribute("name").Value.ToLower().Trim(':'),
                                        Unit =
                                        entry.Attribute("unit") == null
                                            ? null
                                            : entry.Attribute("unit").Value.ToLower() == string.Empty
                                                ? null
                                                : entry.Attribute("unit").Value
                                    }).Distinct().OrderBy(entry => entry.Name).ToList();

                    foreach (var parameter in xmlParameters)
                    {
                        var cat = categories.FirstOrDefault(entry => entry.ExternalIds == categoryGroupParams.Key);
                        if (cat == null)
                        {
                            continue;
                        }
                        var productParameter = new ProductParameter()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = parameter.Name.Truncate(64),
                            UrlName = parameter.Name.Translit().Truncate(64),
                            MeasureUnit = parameter.Unit,
                            CategoryId = cat.Id,
                            AddedBy = "YmlImport",
                            DisplayInFilters = parameter.Unit == null,
                            IsVerified = true,
                            Type = typeof(string).ToString()
                        };
                        dbProductParameters.Add(productParameter);
                        if (parameter.Unit == null)
                        {
                            var xmlProductParameterValues =
                                xmlProducts.SelectMany(entry => entry.Elements("param"))
                                    .Where(entry => entry.Attribute("name").Value.ToLower() == parameter.Name)
                                    .Select(entry => entry.Value.ToLower())
                                    .Distinct().ToList();

                            var productParameterValues =
                                xmlProductParameterValues.Select(entry => new ProductParameterValue()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    ProductParameterId = productParameter.Id,
                                    IsVerified = true,
                                    ParameterValue = entry.Truncate(64),
                                    ParameterValueUrl = entry.Translit().Truncate(64)
                                });
                            dbProductParameterValues.AddRange(productParameterValues);
                        }
                    }
                }

                dbProductParameters = dbProductParameters.OrderBy(entry => entry.Name).ToList();

                Parallel.ForEach(productIdsToAdd, (productIdToAdd) =>
                {
                    var xmlProduct = xmlProducts.First(entry => entry.Attribute("id").Value == productIdToAdd);
                    var name = HttpUtility.HtmlDecode(xmlProduct.Element("name").Value.Replace("\n", "").Replace("\r", "").Trim()).Truncate(256);
                    var descr = xmlProduct.Element("description").GetValueOrDefault(string.Empty).Replace("\n", "<br/>");
                    var currencyId = xmlProduct.Element("currencyId").Value;
                    var urlName = name.Translit().Truncate(128);
                    var category =
                        categories.FirstOrDefault(entry => entry.ExternalIds == xmlProduct.Element("categoryId").Value);
                    double? oldPrice = null;
                    var oldPriceStr = xmlProduct.Element("oldprice").GetValueOrDefault(null) ??
                                      xmlProduct.Element("price_old").GetValueOrDefault(null);
                    if (oldPriceStr != null)
                    {
                        oldPrice = double.Parse(oldPriceStr);
                        if (oldPrice == 0)
                        {
                            oldPrice = null;
                        }
                    }
                    if (category == null)
                    {
                        return;
                    }
                    var product = new Product()
                    {
                        Id = xmlProduct.Attribute("id").Value,
                        ExternalId = xmlProduct.Element("vendorCode").GetValueOrDefault(null),
                        Name = name,
                        UrlName = urlName,
                        Vendor = xmlProduct.Element("vendor").GetValueOrDefault(null),
                        OriginCountry = xmlProduct.Element("country_of_origin").GetValueOrDefault(null),
                        CategoryId = category.Id,
                        SellerId = sellerId,
                        Description = string.IsNullOrEmpty(descr) ? name : descr,
                        IsWeightProduct = false,
                        Price = double.Parse(xmlProduct.Element("price").Value),
                        OldPrice = oldPrice,
                        CurrencyId = currencies.First(entry => entry.Name == currencyId).Id,
                        AvailabilityState = xmlProduct.Attribute("available").Value == "true"
                            ? ProductAvailabilityState.Available
                            : ProductAvailabilityState.NotInStock,
                        IsActive = true,
                        IsImported = true,
                        DoesCountForShipping = true,
                        LastModified = DateTime.UtcNow,
                        AltText = name.Truncate(100),
                        ShortDescription = name,
                        ModerationStatus = ModerationStatus.IsModerating
                    };
                    var productParams = new List<ProductParameterProduct>();
                    foreach (var param in xmlProduct.Elements("param"))
                    {
                        var paramName = param.Attribute("name").Value.ToLower().Trim(':');
                        var parameter =
                            dbProductParameters.FirstOrDefault(
                                entry => entry.Name == paramName && entry.CategoryId == product.CategoryId);

                        var productParameterValue = new ProductParameterProduct()
                        {
                            ProductId = product.Id,
                            ProductParameterId = parameter.Id,
                            StartValue = param.Value.Translit().Truncate(64),
                            StartText = param.Value.Truncate(64)
                        };
                        productParams.Add(productParameterValue);
                    }

                    productParams = productParams.Distinct(new ProductParameterProductComparer()).ToList();
                    var order = 0;
                    lock (lockObj)
                    {
                        dbProductParameterProducts.AddRange(productParams);
                        productsToAddList.Add(product);
                        imagesToAddList.AddRange(xmlProduct.Elements("picture").Select(xmlImage => new Image()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ImageType = ImageType.ProductGallery,
                            ImageUrl = xmlImage.Value,
                            IsAbsoluteUrl = true,
                            Order = order++,
                            ProductId = product.Id,
                            IsImported = true
                        }));
                    }
                });

                Parallel.ForEach(productIdsToUpdate, (productIdToUpdate) =>
                {
                    var product = dbProducts.FirstOrDefault(entry => entry.Id == productIdToUpdate);
                    var xmlProduct = xmlProducts.First(entry => entry.Attribute("id").Value == productIdToUpdate);
                    var currencyId = xmlProduct.Element("currencyId").Value;
                    var category =
                        categories.FirstOrDefault(entry => entry.ExternalIds == xmlProduct.Element("categoryId").Value);
                    if (category == null)
                    {
                        return;
                    }

                    //var name = HttpUtility.HtmlDecode(xmlProduct.Element("name").Value.Replace("\n", "").Replace("\r", "").Trim()).Truncate(256);
                    //var descr = xmlProduct.Element("description").GetValueOrDefault(string.Empty).Replace("\n", "<br/>");
                    //var currencyId = xmlProduct.Element("currencyId").Value;
                    double? oldPrice = null;
                    var oldPriceStr = xmlProduct.Element("oldprice").GetValueOrDefault(null) ??
                                      xmlProduct.Element("price_old").GetValueOrDefault(null);
                    if (oldPriceStr != null)
                    {
                        oldPrice = double.Parse(oldPriceStr);
                        if (oldPrice == 0)
                        {
                            oldPrice = null;
                        }
                    }
                    product.CurrencyId = currencies.First(entry => entry.Name == currencyId).Id;

                    //product.Name = name;
                    product.ExternalId = xmlProduct.Element("vendorCode").GetValueOrDefault(null);
                    //product.UrlName = name.Translit().Truncate(128);
                    //product.CategoryId = categories.FirstOrDefault(entry => entry.ExternalIds == xmlProduct.Element("categoryId").Value).Id;
                    //product.Description = string.IsNullOrEmpty(descr) ? name : descr;
                    product.Price = double.Parse(xmlProduct.Element("price").Value);
                    product.OldPrice = oldPrice;
                    //product.CurrencyId = currencies.First(entry => entry.Name == currencyId).Id;

                    product.AvailabilityState = xmlProduct.Attribute("available").Value == "true"
                        ? ProductAvailabilityState.Available
                        : ProductAvailabilityState.OnDemand;
                    product.LastModified = DateTime.UtcNow;
                    //product.AltText = name.Truncate(100);
                    //product.ShortDescription = name;

                    //var productParams = new List<ProductParameterProduct>();
                    //foreach (var param in xmlProductsToAdd.Elements("param"))
                    //{
                    //    var paramName = param.Attribute("name").Value.ToLower().Trim(':');
                    //    var parameter =
                    //        dbProductParameters.FirstOrDefault(
                    //            entry => entry.Name == paramName && entry.CategoryId == product.CategoryId);

                    //    var productParameterValue = new ProductParameterProduct()
                    //    {
                    //        ProductId = product.Id,
                    //        ProductParameterId = parameter.Id,
                    //        StartValue = param.Value.Translit().Truncate(64),
                    //        StartText = param.Value.Truncate(64)
                    //    };
                    //    productParams.Add(productParameterValue);
                    //}

                    //productParams = productParams.Distinct(new ProductParameterProductComparer()).ToList();

                    var order = 0;
                    lock (lockObj)
                    {
                        var existingImages = db.Images.Where(entry => entry.ProductId == product.Id).Select(entry => entry.ImageUrl);
                        var newImages = xmlProduct.Elements("picture").Select(xmlImage => new Image()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ImageType = ImageType.ProductGallery,
                            ImageUrl = xmlImage.Value,
                            IsAbsoluteUrl = true,
                            Order = order++,
                            ProductId = product.Id,
                            IsImported = true
                        }).Where(entry => !existingImages.Contains(entry.ImageUrl));
                        imagesToAddList.AddRange(newImages);
                        //dbProductParameterProducts.AddRange(productParams);
                    }
                });

                foreach (var product in productsToAddList)
                {
                    product.SKU = maxSku;
                    product.UrlName = product.UrlName.Insert(0, maxSku++ + "_").Truncate(128);
                }
                productsToAddList = productsToAddList.Distinct(new ProductComparer()).ToList();
                db.InsertIntoMembers(productsToAddList);
                db.SaveChanges();
                db.InsertIntoMembers(imagesToAddList);
                db.InsertIntoMembers(dbProductParameters);
                db.InsertIntoMembers(dbProductParameterValues);
                dbProductParameterProducts = dbProductParameterProducts.Where(entry => entry != null).Distinct(new ProductParameterProductComparer()).ToList();
                db.InsertIntoMembers(dbProductParameterProducts);
                var mappedProductParameters =
                    db.MappedProductParameters.Where(entry => entry.SellerId == sellerId).ToList();
                foreach (var mappedProductParameter in mappedProductParameters)
                {
                    var pp = dbProductParameters.FirstOrDefault(entry => entry.Name == mappedProductParameter.Name);
                    if (pp != null)
                    {
                        pp.ParentProductParameterId = mappedProductParameter.ProductParameterId;
                    }
                }
                foreach (var image in imagesToAddList.Where(entry => entry.ImageUrl.Contains(SettingsService.BaseHostName)))
                {
                    var uri = new Uri(image.ImageUrl);
                    var path = originalDirectory + uri.LocalPath;
                    ImagesService.ResizeToSiteRatio(path, ImageType.ProductGallery);
                }
            }
        }

        private void DeletePromUaProducts(List<XElement> xmlProducts, string sellerId, SyncType importType, ApplicationDbContext db)
        {
            var currentSellerProductIds = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported)
                .Select(entry => entry.Id).ToList();
            List<string> xmlProductIds = null;
            if (importType == SyncType.OneCCommerceMl)
            {
                xmlProductIds = xmlProducts.Select(entry => entry.Element("Ид").Value).ToList();
            }
            if (importType == SyncType.Yml)
            {
                xmlProductIds = xmlProducts.Select(entry => entry.Attribute("id").Value).ToList();
            }
            var productIdsToRemove = currentSellerProductIds.Except(xmlProductIds).Where(entry => entry != null).ToList();
            var productsToRemove = db.Products.Where(entry => entry.SellerId == sellerId && productIdsToRemove.Contains(entry.Id)).ToList();
            Parallel.ForEach(productsToRemove, (dbProduct) =>
            {
                dbProduct.AvailabilityState = ProductAvailabilityState.NotInStock;
            });
        }

        public void ImportFromYml(string importTaskId)
        {
            using (var db = new ApplicationDbContext())
            {
                var importTask = db.ExportImports
                .Include(entry => entry.Seller)
                .FirstOrDefault(entry => entry.Id == importTaskId);
                if (importTask.IsImport == true) return;
                //show that import task is processing
                importTask.IsImport = true;
                db.SaveChanges();
                _logger.Info(string.Format("Yml Import started at {0} {1}", importTask.Seller.Id, importTask.Seller.UrlName));

                XDocument xml = null;
                try
                {
                    xml = XDocument.Load(importTask.FileUrl);
                }
                catch (Exception ex)
                {
                    importTask.LastUpdateStatus = false;
                    importTask.LastUpdateMessage = "Неможливо обробити файл, перевірте правильність посилання на файл";
                    importTask.LastSync = DateTime.UtcNow;
                    db.Entry(importTask).State = EntityState.Modified;
                    db.SaveChanges();
                    return;
                }

                try
                {
                    var root = xml.Element("yml_catalog").Element("shop");
                    var xmlCategories = root.Descendants("categories").First().Elements().ToList();
                    var xmlCategoryIds = xmlCategories.Select(entry => entry.Attribute("id").Value).ToList();
                    CreateAndUpdateYmlCategories(xmlCategories, importTask.Seller.UrlName, importTask.Seller.Id, db);
                    db.SaveChanges();
                    DeleteImportCategories(importTask.Seller, xmlCategories, SyncType.Yml, db);
                    db.SaveChanges();

                    var xmlProducts = root.Descendants("offers").First().Elements().ToList();
                    AddAndUpdateYmlProducts(xmlProducts, importTask.SellerId, xmlCategoryIds);
                    db.SaveChanges();
                    DeletePromUaProducts(xmlProducts, importTask.SellerId, SyncType.Yml, db);
                    db.SaveChanges();

                    //todo:handle exceptions

                    importTask.LastUpdateStatus = true;
                    importTask.LastUpdateMessage = null;
                    _logger.Info(string.Format("Yml Import success at {0} {1}", importTask.Seller.Id, importTask.Seller.Name));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    importTask.LastUpdateStatus = false;
                    importTask.LastUpdateMessage =
                        "У ході обробки файлу виникли помилки, будь ласка зверніться до служби підтримки";
                }
                finally
                {
                    importTask.IsImport = false;
                    importTask.LastSync = DateTime.UtcNow;
                    db.Entry(importTask).State = EntityState.Modified;
                    db.SaveChanges();
                    _logger.Info(string.Format("Yml results saved {0} {1}", importTask.Seller.Id, importTask.Seller.Name));
                }
            }
        }

        public void ProcessImportTasks()
        {
            using (var db = new ApplicationDbContext())
            {
                var importTasks =
                db.ExportImports.Include(entry => entry.Seller.MappedCategories).Where(
                    entry => entry.IsActive && (entry.SyncType == SyncType.Yml || entry.SyncType == SyncType.Gbs)).ToList();
                foreach (var importTask in importTasks)
                {
                    if (importTask.LastSync.HasValue &&
                        (DateTime.UtcNow - importTask.LastSync.Value).TotalDays < importTask.SyncPeriod)
                    {
                        continue;
                    }
                    if (importTask.SyncType == SyncType.Yml)
                    {
                        ImportFromYml(importTask.Id);
                    }
                    if (importTask.SyncType == SyncType.Gbs)
                    {
                        var gbsService = new GbsImportService();
                        gbsService.Import(importTask.Id);
                    }
                }
            }
        }

        #endregion

        #region Excel

        private void ProcessExcelCategories(IEnumerable<string> allCats, List<Category> allDbCats, string sellerId, string parentName, ApplicationDbContext db, string parentId = null)
        {

            var cats = allCats.Select(entry =>
        {
            var startIndex = parentName == null ? 0 : entry.IndexOf(parentName) + parentName.Length + 1;
            var indexOfSlash = entry.IndexOf("/", startIndex) > 0
                ? entry.IndexOf("/", startIndex)
                : entry.Length;
            return entry.Substring(startIndex, indexOfSlash - startIndex);
        }).Distinct().ToList();
            foreach (var cat in cats)
            {
                var dbParentCat =
                    db.Categories.FirstOrDefault(entry => entry.Name == cat && entry.SellerId == sellerId);
                if (dbParentCat == null)
                {
                    dbParentCat = new Category()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = cat,
                        UrlName = string.Format("{0}_{1}", sellerId, cat.Translit().Truncate(128 - sellerId.Length)),
                        ParentCategoryId = parentId,
                        IsSellerCategory = true,
                        SellerId = sellerId,
                        IsActive = true,
                        LastModified = DateTime.UtcNow,
                        LastModifiedBy = "Excel import"
                    };
                    db.Categories.Add(dbParentCat);
                }
                allDbCats.Add(dbParentCat);
                var xlsCat = allCats.FirstOrDefault(entry => entry.Contains(cat));
                if (xlsCat.IndexOf(cat) + cat.Length == xlsCat.Length)
                {
                    dbParentCat.ExpandedSlashName = xlsCat;
                }

                var childsCats = allCats.Where(entry => entry.Contains(cat + "/")).ToList();
                ProcessExcelCategories(childsCats, allDbCats, sellerId, cat, db, dbParentCat.Id);
            }
        }

        public async Task<ProductImportResults> ImportFromExcel(string sellerId, string xlsPath)
        {
            using (var db = new ApplicationDbContext())
            {
                var result = new ProductImportResults();
                var catalog = new LinqToExcel.ExcelQueryFactory(xlsPath);
                var worksheetName = catalog.GetWorksheetNames().FirstOrDefault();
                if (worksheetName == null)
                {
                    throw new Exception("Файл імпорту не містить листів");
                }

                var worksheet = catalog.Worksheet(worksheetName);
                var rows = worksheet.ToList().Where(entry =>
                        !string.IsNullOrEmpty(entry["Category"].Value.ToString()) &&
                        !string.IsNullOrEmpty(entry["SKU"].Value.ToString()))
                    .ToList();
                var excelProducts =
                    (from row in rows
                     let item = new ExcelProduct()
                     {
                         Product = new Product
                         {
                             ExternalId = row["SKU"].Cast<string>(),
                             Vendor = row["Brand"].Cast<string>(),
                             Name = row["Product"].Cast<string>().Truncate(256),
                             Price = row["Price"].Cast<double>(),
                             OldPrice = row["Old price"].Cast<double>(),
                             IsActive = true,
                             IsWeightProduct = row["Units"].Cast<string>() != "шт",
                             IsFeatured = row["Featured"].Cast<bool>(),
                             Title = row["Meta title"].Cast<string>().Truncate(70),
                             Description = row["Description"].Cast<string>(),
                             ShortDescription = row["Meta description"].Cast<string>().Truncate(160),
                             UrlName = row["URL"].Cast<string>().Truncate(128) ?? row["Product"].Cast<string>().Translit().Truncate(128),
                             AvailableAmount = row["Stock"].Cast<int?>()
                         },
                         CategoryName = row["Category"].Cast<string>().Trim().Replace("\n", string.Empty)
                             .Replace("\t", string.Empty),
                         ImagesList = row["Images"].Cast<string>(),
                         CurrencyName = row["Currency"].Cast<string>(),
                         Visible = row["Visible"].Cast<int>()
                     }
                     select item).ToList()
                    .Where(entry => !string.IsNullOrEmpty(entry.CategoryName)).ToList();

                #region Excel Categories

                var allCats = excelProducts
                    .Select(entry => entry.CategoryName.Trim().Replace("\n", string.Empty).Replace("\t", string.Empty))
                    .Distinct().ToList();
                var allDbCats = new List<Category>();
                ProcessExcelCategories(allCats, allDbCats, sellerId, null, db);
                var alldDbNames = allDbCats.Select(entry => entry.Name).ToList();
                var inActiveCategories = db.Categories
                    .Where(entry => entry.SellerId == sellerId && !alldDbNames.Contains(entry.Name)).ToList();
                inActiveCategories.ForEach(entry => categoriesService.Delete(entry.Id));
                db.SaveChanges();

                #endregion

                #region Parameters

                var productParametersToAdd = new List<ProductParameter>();
                var productParameterValuesToAdd = new List<ProductParameterValue>();
                var productParameterProductsToAdd = new List<ProductParameterProduct>();

                var existingCategories = db.Categories.Where(entry => entry.SellerId == sellerId);
                var existingCategoryIds = existingCategories.Select(entry => entry.Id).ToList();
                var existingProductParameters =
                    db.ProductParameters.Where(
                        entry => existingCategoryIds.Contains(entry.CategoryId)).ToList();
                var productParameterIds = existingProductParameters.Select(pr => pr.Id).ToList();
                var existingProductParameterValues =
                    db.ProductParameterValues.Where(
                        entry => productParameterIds.Contains(entry.ProductParameterId)).ToList();
                var existingProductParameterProducts = db.ProductParameterProducts.Where(
                    entry => productParameterIds.Contains(entry.ProductParameterId)).ToList();

                db.DeleteWhereColumnIn(existingProductParameterProducts, "ProductParameterId");
                db.DeleteWhereColumnIn(existingProductParameterValues);
                db.DeleteWhereColumnIn(existingProductParameters);
                #endregion

                var currencies = db.Currencies.AsNoTracking().ToList();
                var xlsProducts = new List<Product>();
                excelProducts.ForEach(entry =>
                {
                    var product = entry.Product;
                    if (product.Description == null)
                    {
                        product.Description = product.Name;
                    }

                    product.CategoryId = allDbCats.First(cat => entry.CategoryName.Contains(cat.Name)).Id;
                    var curr = currencies.FirstOrDefault(cur => cur.Name == entry.CurrencyName);
                    product.CurrencyId = curr == null ? null : curr.Id;
                    product.OldPrice = product.OldPrice == default(double) ? (double?)null : product.OldPrice.Value;
                    product.AvailabilityState = entry.Visible > 0 ? ProductAvailabilityState.Available : ProductAvailabilityState.NotInStock;
                    xlsProducts.Add(product);
                });

                var dbProductsIds =
                    db.Products.Where(entry => entry.SellerId == sellerId).Select(entry => entry.ExternalId).Where(entry => entry != null).ToList();
                var xlsProductIds = xlsProducts.Select(entry => entry.ExternalId).ToList();

                var productIdsToDelete = dbProductsIds.Except(xlsProductIds).ToList();
                result.ProductsRemoved = productIdsToDelete.Count;
                db.Products.Where(entry => productIdsToDelete.Contains(entry.ExternalId))
                    .ToList()
                    .ForEach(entry =>
                    {
                        entry.AvailabilityState = ProductAvailabilityState.NotInStock;
                        db.Entry(entry).State = EntityState.Modified;
                    });

                var productExternalIdsToAdd = xlsProductIds.Except(dbProductsIds).ToList();
                Dictionary<string, string> productIdsToAdd = new Dictionary<string, string>(), productIdsToUpdate = new Dictionary<string, string>();
                if (productExternalIdsToAdd.Any())
                {
                    result.ProductsAdded = productExternalIdsToAdd.Count;
                    var sku = (db.Products.Max(pr => (int?)pr.SKU) ?? SettingsService.SkuMinValue) + 1;
                    var productsToAdd = new List<Product>();
                    xlsProducts.Where(entry => productExternalIdsToAdd.Contains(entry.ExternalId)).ToList().ForEach(entry =>
                    {
                        entry.Id = Guid.NewGuid().ToString();
                        entry.SKU = sku++;
                        entry.LastModified = DateTime.UtcNow;
                        entry.SellerId = sellerId;
                        productIdsToAdd.Add(entry.ExternalId, entry.Id);
                        productsToAdd.Add(entry);
                    });
                    db.Products.AddRange(productsToAdd);
                }

                var productExternalIdsToUpdate = xlsProductIds.Intersect(dbProductsIds).ToList();
                if (productExternalIdsToUpdate.Any())
                {
                    result.ProductsUpdated = productExternalIdsToUpdate.Count;
                    var xlsProductsToUpdate = xlsProducts.Where(entry => productExternalIdsToUpdate.Contains(entry.ExternalId)).ToList();
                    var dbProductsToUpdate = db.Products.Where(entry => productExternalIdsToUpdate.Contains(entry.ExternalId) && entry.SellerId == sellerId);
                    productIdsToUpdate = dbProductsToUpdate.ToDictionary(entry => entry.ExternalId, entry => entry.Id);
                    Parallel.ForEach(dbProductsToUpdate, (dbProduct) =>
                    {
                        var xlsProductToUpdate = xlsProductsToUpdate.First(entry => entry.ExternalId == dbProduct.ExternalId);
                        xlsProductToUpdate.Id = dbProduct.Id;

                        dbProduct.CategoryId = xlsProductToUpdate.CategoryId;
                        dbProduct.Vendor = xlsProductToUpdate.Vendor;
                        dbProduct.Name = xlsProductToUpdate.Name;
                        dbProduct.Title = xlsProductToUpdate.Title;
                        dbProduct.UrlName = xlsProductToUpdate.UrlName;
                        dbProduct.Price = xlsProductToUpdate.Price;
                        dbProduct.OldPrice = xlsProductToUpdate.OldPrice;
                        dbProduct.CurrencyId = xlsProductToUpdate.CurrencyId;
                        dbProduct.IsWeightProduct = xlsProductToUpdate.IsWeightProduct;
                        dbProduct.AvailabilityState = xlsProductToUpdate.AvailabilityState;
                        dbProduct.AvailableAmount = xlsProductToUpdate.AvailableAmount;
                        dbProduct.ShortDescription = xlsProductToUpdate.ShortDescription;
                        dbProduct.Description = xlsProductToUpdate.Description;
                        dbProduct.LastModified = DateTime.UtcNow;
                        dbProduct.SellerId = sellerId;
                    });
                }

                var columnNames = catalog.GetColumnNames(worksheetName).ToList();
                var parameterNames = columnNames.GetRange(columnNames.IndexOf("URL") + 1, columnNames.Count - columnNames.IndexOf("URL") - 1);

                foreach (var parameterName in parameterNames)
                {
                    var catsWithParam = rows.Where(entry => !string.IsNullOrEmpty(entry[parameterName].Value.ToString()))
                        .Select(entry => entry["Category"].Value.ToString().Trim().Replace("\n", string.Empty)).Distinct().ToList();
                    var dbCatsWithParam =
                        allDbCats.Where(entry => catsWithParam.Contains(entry.ExpandedSlashName)).ToList();
                    foreach (var cat in dbCatsWithParam)
                    {
                        var productParameter = new ProductParameter()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = parameterName.Truncate(64),
                            UrlName = parameterName.Translit().Truncate(64),
                            CategoryId = cat.Id,
                            CategoryName = cat.Name,
                            AddedBy = "XlsImport",
                            DisplayInFilters = true,
                            IsVerified = true,
                            Type = typeof(string).ToString()
                        };
                        productParametersToAdd.Add(productParameter);

                        var xmlProductParameterValues =
                            rows.Where(entry => !string.IsNullOrEmpty(entry[parameterName].Value.ToString()))
                                .Select(entry =>
                                    entry[parameterName].Value.ToString().Trim().Replace("\n", string.Empty)
                                        .Replace("\t", string.Empty)).Where(entry => !string.IsNullOrEmpty(entry))
                                .Distinct().ToList();

                        var productParameterValues =
                            xmlProductParameterValues.Select(entry => new ProductParameterValue()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ProductParameterId = productParameter.Id,
                                IsVerified = true,
                                ParameterValue = entry.Truncate(64),
                                ParameterValueUrl = entry.Translit().Truncate(64)
                            });
                        productParameterValuesToAdd.AddRange(productParameterValues);
                    }

                    var productParameterRows =
                        rows.Where(entry => !string.IsNullOrEmpty(entry[parameterName].Value.ToString())).ToList();
                    foreach (var productParameterRow in productParameterRows)
                    {
                        var product = excelProducts.FirstOrDefault(entry =>
                            entry.Product.ExternalId == productParameterRow["SKU"].Cast<string>());
                        var pp = productParametersToAdd.FirstOrDefault(entry => entry.Name == parameterName && entry.CategoryName == productParameterRow["Category"].Value.ToString().Clear());
                        var productParameterProduct = new ProductParameterProduct()
                        {
                            ProductId = product.Product.Id,
                            ProductParameterId = pp.Id,
                            StartValue = productParameterRow[parameterName].Value.ToString().Translit().Truncate(64),
                            StartText = productParameterRow[parameterName].Value.ToString().Truncate(64)
                        };
                        productParameterProductsToAdd.Add(productParameterProduct);
                    }
                }

                // map seller parameters to site parameters
                var mappedProductParameters =
                    db.MappedProductParameters.Where(entry => entry.SellerId == sellerId).ToList();
                foreach (var mappedProductParameter in mappedProductParameters)
                {
                    var pp = productParametersToAdd.FirstOrDefault(entry => entry.Name == mappedProductParameter.Name);
                    if (pp != null)
                    {
                        pp.ParentProductParameterId = mappedProductParameter.ProductParameterId;
                    }
                }
                db.ProductParameters.AddRange(productParametersToAdd);
                db.ProductParameterValues.AddRange(productParameterValuesToAdd);
                db.ProductParameterProducts.AddRange(productParameterProductsToAdd);
                db.SaveChanges();
                //images
                var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                var path = new DirectoryInfo(xlsPath);
                var imagesPath = path.Parent.GetDirectories("images", SearchOption.AllDirectories)
                        .FirstOrDefault();
                var imageType = ImageType.ProductGallery;
                var allProductIds = productIdsToAdd.Concat(productIdsToUpdate)
                    .ToDictionary(entry => entry.Key, entry => entry.Value);
                foreach (var excelProduct in excelProducts.Where(entry => !string.IsNullOrEmpty(entry.ImagesList)))
                {
                    var productId = allProductIds[excelProduct.Product.ExternalId];
                    ImagesService.DeleteAll(db.Images.Where(entry => entry.ProductId == productId).ToList(), productId,
                                            imageType);
                    var destPath = Path.Combine(originalDirectory, "Images", imageType.ToString(), productId);
                    var isExists = Directory.Exists(destPath);
                    if (!isExists)
                    {
                        Directory.CreateDirectory(destPath);
                    }

                    var index = 0;
                    var imgs = excelProduct.ImagesList.Split(',').Where(entry => !string.IsNullOrEmpty(entry));
                    foreach (var img in imgs)
                    {
                        var fileName = img;
                        using (var client = new WebClient())
                        {
                            if (Uri.IsWellFormedUriString(img, UriKind.Absolute))
                            {
                                fileName = Path.GetFileName(img);
                                try
                                {
                                    client.DownloadFile(img, Path.Combine(destPath, fileName));
                                    ImagesService.ResizeToSiteRatio(Path.Combine(destPath, fileName),
                                        ImageType.ProductGallery);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.ToString());
                                }
                            }
                            else
                            {
                                if (imagesPath.Exists)
                                {
                                    var ftpImage = new FileInfo(Path.Combine(imagesPath.FullName, fileName));
                                    if (ftpImage.Exists)
                                    {
                                        ftpImage.CopyTo(Path.Combine(destPath, ftpImage.Name), true);
                                        ImagesService.ResizeToSiteRatio(Path.Combine(destPath, ftpImage.Name),
                                            ImageType.ProductGallery);
                                    }
                                }
                            }
                            ImagesService.AddImage(productId, fileName, imageType, index++);
                        }
                    }
                }
                return result;
            }
            #endregion
        }
    } 
}
