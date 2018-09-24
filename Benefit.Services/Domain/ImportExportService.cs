using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Excel;
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
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Benefit.Services.Domain
{
    public class ImportExportService
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private CategoriesService categoriesService = new CategoriesService();
        private ProductsService productsService = new ProductsService();
        private ImagesService ImagesService = new ImagesService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
        object lockObj = new object();

        #region 1C

        public bool ImportFrom1C(XDocument xml, Seller seller)
        {
            try
            {
                var rawXmlCategories = xml.Descendants("Группы").First().Elements().ToList();
                var resultXmlCategories = GetAllFiniteCategories(rawXmlCategories);
                var resultXmlCategoryIds = resultXmlCategories.Select(entry => entry.Element("Ид").Value);

                CreateAndUpdate1CCategories(resultXmlCategories, seller.Id);
                db.SaveChanges();
                DeleteImportCategories(seller, resultXmlCategories, SyncType.OneCCommerceMl);
                db.SaveChanges();

                var xmlProducts = xml.Descendants("Товары").First().Elements()
                    .Where(entry => entry.Element("Группы") != null)
                    .Where(entry => resultXmlCategoryIds.Contains(entry.Element("Группы").Element("Ид").Value)).ToList();
                AddAndUpdate1СProducts(xmlProducts, seller.Id, seller.UrlName);
                db.SaveChanges();
                DeletePromUaProducts(xmlProducts, seller.Id, SyncType.OneCCommerceMl);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
            return true;
        }

        private void AddAndUpdate1СProducts(List<XElement> xmlProducts, string sellerId, string sellerUrl)
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

            var existingImages = db.Images.Where(entry => dbProductIds.Contains(entry.ProductId) && entry.IsImported).ToList();
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
                    LastModified = DateTime.UtcNow,
                    LastModifiedBy = "1CImport",
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
                product.LastModifiedBy = "1CUaImport";

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

        private void CreateAndUpdate1CCategories(List<XElement> xmlCategories, string sellerId)
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

        private void CreateAndUpdatePromUaCategories(List<XElement> xmlCategories, string sellerUrlName,
            string sellerId, Category parent = null)
        {
            var hasNewContent = false;
            List<XElement> xmlCats = null;
            if (parent == null)
            {
                xmlCats = xmlCategories.Where(entry => entry.Attribute("parentId") == null).ToList();
            }
            else
            {
                xmlCats = xmlCategories.Where(entry =>
                    entry.Attribute("parentId") != null && entry.Attribute("parentId").Value == parent.Id).ToList();
            }

            foreach (var xmlCategory in xmlCats)
            {
                var catId = xmlCategory.Attribute("id").Value;
                var catName = xmlCategory.Value.Replace("\n", "").Replace("\r", "").Trim();
                var dbCategory = db.Categories.FirstOrDefault(entry => entry.Id == catId);
                if (dbCategory == null)
                {
                    dbCategory = new Category()
                    {
                        Id = catId,
                        ParentCategoryId = xmlCategory.Attribute("parentId") == null
                            ? null
                            : xmlCategory.Attribute("parentId").Value,
                        IsSellerCategory = true,
                        SellerId = sellerId,
                        Name = catName.Truncate(64),
                        UrlName = string.Format("{0}_{1}", catId, catName.Translit()).Truncate(128),
                        Description = catName,
                        MetaDescription = catName,
                        IsActive = true,
                        LastModified = DateTime.UtcNow,
                        LastModifiedBy = "ImportFromPromua"
                    };
                    db.Categories.Add(dbCategory);
                }
                else
                {
                    if (!hasNewContent)
                    {
                        hasNewContent = true;
                    }

                    dbCategory.Name = catName.Truncate(64);
                    dbCategory.UrlName = string.Format("{0}_{1}", catId, catName.Translit()).Truncate(128);

                    dbCategory.ParentCategoryId = xmlCategory.Attribute("parentId") == null
                        ? null
                        : xmlCategory.Attribute("parentId").Value;
                    db.Entry(dbCategory).State = EntityState.Modified;
                }

                CreateAndUpdatePromUaCategories(xmlCategories, sellerUrlName, sellerId, dbCategory);
            }
            if (hasNewContent)
            {
                var importTask = db.ExportImports.FirstOrDefault(entry => entry.SellerId == sellerId);
                importTask.HasNewContent = true;
            }
        }

        private void DeleteImportCategories(Seller seller, IEnumerable<XElement> xmlCategories, SyncType importType)
        {
            var currentSellercategoyIds = seller.MappedCategories.Select(entry => entry.Id).ToList();
            List<string> xmlCategoryIds = null;
            if (importType == SyncType.OneCCommerceMl)
            {
                xmlCategoryIds = xmlCategories.Select(entry => entry.Element("Ид").Value).ToList();
            }
            if (importType == SyncType.Yml)
            {
                xmlCategoryIds = xmlCategories.Select(entry => entry.Attribute("id").Value).ToList();
            }
            var catIdsToRemove = currentSellercategoyIds.Except(xmlCategoryIds).ToList();
            foreach (var catId in catIdsToRemove)
            {
                var dbCategory = db.Categories.Find(catId);
                dbCategory.IsActive = false;
                db.Entry(dbCategory).State = EntityState.Modified;
            }
        }

        private void AddAndUpdateYmlProducts(List<XElement> xmlProducts, string sellerId)
        {
            var maxSku = db.Products.Max(entry => entry.SKU) + 1;
            var xmlProductIds = xmlProducts.Select(entry => entry.Attribute("id").Value).ToList();
            var dbProducts = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported).ToList();
            var dbProductIds = dbProducts.Select(entry => entry.Id).ToList();
            var productIdsToAdd = xmlProductIds.Where(entry => !dbProductIds.Contains(entry)).ToList();
            var productIdsToUpdate = xmlProductIds.Where(dbProductIds.Contains).ToList();

            var productsToAddList = new List<Product>();
            var imagesToAddList = new List<Image>();

            var existingImages = db.Images.Where(entry => dbProductIds.Contains(entry.ProductId)).ToList();
            var currencies = db.Currencies.Where(entry => entry.SellerId == null || entry.SellerId == sellerId)
                .ToList();

            //parameters
            var productParametersToAdd = new List<ProductParameter>();
            var productParameterValuesToAdd = new List<ProductParameterValue>();
            var productParameterProductsToAdd = new List<ProductParameterProduct>();
            var productsGroupByCategoryId = xmlProducts.GroupBy(entry => entry.Element("categoryId").Value).ToList();

            var categryIds = productsGroupByCategoryId.Select(pr => pr.Key).ToList();
            var existingProductParameters =
                db.ProductParameters.Where(
                    entry => categryIds.Contains(entry.CategoryId)).ToList();
            var productParameterIds = existingProductParameters.Select(pr => pr.Id).ToList();
            var existingProductParameterValues =
                db.ProductParameterValues.Where(
                    entry => productParameterIds.Contains(entry.ProductParameterId)).ToList();
            var existingProductParameterProducts = db.ProductParameterProducts.Where(
                entry => productParameterIds.Contains(entry.ProductParameterId)).ToList();

            db.DeleteWhereColumnIn(existingImages);
            db.DeleteWhereColumnIn(existingProductParameterProducts, "ProductParameterId");
            db.DeleteWhereColumnIn(existingProductParameterValues);
            db.DeleteWhereColumnIn(existingProductParameters);

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
                    var productParameter = new ProductParameter()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = parameter.Name.Truncate(64),
                        UrlName = parameter.Name.Translit().Truncate(64),
                        MeasureUnit = parameter.Unit,
                        CategoryId = categoryGroupParams.Key,
                        AddedBy = "YmlImport",
                        DisplayInFilters = parameter.Unit == null,
                        IsVerified = true,
                        Type = typeof(string).ToString()
                    };
                    productParametersToAdd.Add(productParameter);
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
                        productParameterValuesToAdd.AddRange(productParameterValues);
                    }
                }
            }

            productParametersToAdd = productParametersToAdd.OrderBy(entry => entry.Name).ToList();

            Parallel.ForEach(productIdsToAdd, (productIdToAdd) =>
            {
                var xmlProduct = xmlProducts.First(entry => entry.Attribute("id").Value == productIdToAdd);
                var name = HttpUtility.HtmlDecode(xmlProduct.Element("name").Value.Replace("\n", "").Replace("\r", "").Trim()).Truncate(256);
                var descr = xmlProduct.Element("description").GetValueOrDefault(string.Empty).Replace("\n", "<br/>");
                var currencyId = xmlProduct.Element("currencyId").Value;
                var urlName = name.Translit().Truncate(128);
                var product = new Product()
                {
                    Id = xmlProduct.Attribute("id").Value,
                    ExternalId = xmlProduct.Element("vendorCode").GetValueOrDefault(null),
                    Name = name,
                    UrlName = urlName,
                    Vendor = xmlProduct.Element("vendor").GetValueOrDefault(null),
                    OriginCountry = xmlProduct.Element("country_of_origin").GetValueOrDefault(null),
                    CategoryId = xmlProduct.Element("categoryId").Value,
                    SellerId = sellerId,
                    Description = string.IsNullOrEmpty(descr) ? name : descr,
                    IsWeightProduct = false,
                    Price = double.Parse(xmlProduct.Element("price").Value),
                    CurrencyId = currencies.First(entry => entry.Name == currencyId).Id,
                    AvailabilityState = xmlProduct.Attribute("available").Value == "true"
                        ? ProductAvailabilityState.Available
                        : ProductAvailabilityState.OnDemand,
                    IsActive = true,
                    IsImported = true,
                    DoesCountForShipping = true,
                    LastModified = DateTime.UtcNow,
                    LastModifiedBy = "PromUaImport",
                    AltText = name.Truncate(100),
                    ShortDescription = name
                };
                var productParams = new List<ProductParameterProduct>();
                foreach (var param in xmlProduct.Elements("param"))
                {
                    var paramName = param.Attribute("name").Value.ToLower().Trim(':');
                    var parameter =
                        productParametersToAdd.FirstOrDefault(
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
                    productParameterProductsToAdd.AddRange(productParams);
                    productsToAddList.Add(product);
                    imagesToAddList.AddRange(xmlProduct.Elements("picture").Select(xmlImage => new Image()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ImageType = ImageType.ProductGallery,
                        ImageUrl = xmlImage.Value,
                        IsAbsoluteUrl = true,
                        Order = order++,
                        ProductId = product.Id
                    }));
                }
            });

            Parallel.ForEach(productIdsToUpdate, (productIdToUpdate) =>
            {
                var product = dbProducts.FirstOrDefault(entry => entry.Id == productIdToUpdate);
                var xmlProduct = xmlProducts.First(entry => entry.Attribute("id").Value == productIdToUpdate);

                var name = HttpUtility.HtmlDecode(xmlProduct.Element("name").Value.Replace("\n", "").Replace("\r", "").Trim()).Truncate(256);
                var descr = xmlProduct.Element("description").GetValueOrDefault(string.Empty).Replace("\n", "<br/>");
                var currencyId = xmlProduct.Element("currencyId").Value;

                product.Name = name;
                product.ExternalId = xmlProduct.Element("vendorCode").GetValueOrDefault(null);
                product.UrlName = name.Translit().Truncate(128);
                product.CategoryId = xmlProduct.Element("categoryId").Value;
                product.Description = string.IsNullOrEmpty(descr) ? name : descr;
                product.Price = double.Parse(xmlProduct.Element("price").Value);
                product.CurrencyId = currencies.First(entry => entry.Name == currencyId).Id;

                product.AvailabilityState = xmlProduct.Attribute("available").Value == "true"
                    ? ProductAvailabilityState.Available
                    : ProductAvailabilityState.OnDemand;
                product.LastModified = DateTime.UtcNow;
                product.LastModifiedBy = "PromUaImport";
                product.AltText = name.Truncate(100);
                product.ShortDescription = name;

                var order = 0;

                lock (lockObj)
                {
                    imagesToAddList.AddRange(xmlProduct.Elements("picture").Select(xmlImage => new Image()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ImageType = ImageType.ProductGallery,
                        ImageUrl = xmlImage.Value,
                        IsAbsoluteUrl = true,
                        Order = order++,
                        ProductId = product.Id
                    }));
                }

                var productParams = new List<ProductParameterProduct>();
                foreach (var param in xmlProduct.Elements("param"))
                {
                    var paramName = param.Attribute("name").Value.ToLower().Trim(':');
                    var parameter =
                        productParametersToAdd.FirstOrDefault(
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
                productParameterProductsToAdd.AddRange(productParams);
            });

            foreach (var product in productsToAddList)
            {
                product.SKU = maxSku;
                product.UrlName = product.UrlName.Insert(0, maxSku++ + "_").Truncate(128);
            }

            db.InsertIntoMembers(productsToAddList);
            db.SaveChanges();
            db.InsertIntoMembers(imagesToAddList);
            db.InsertIntoMembers(productParametersToAdd);
            db.InsertIntoMembers(productParameterValuesToAdd);
            productParameterProductsToAdd = productParameterProductsToAdd.Where(entry => entry != null).ToList();
            db.InsertIntoMembers(productParameterProductsToAdd);
            foreach (var image in imagesToAddList.Where(entry => entry.ImageUrl.Contains(SettingsService.BaseHostName)))
            {
                var uri = new Uri(image.ImageUrl);
                var path = originalDirectory + uri.LocalPath;
                ImagesService.ResizeToSiteRatio(path, ImageType.ProductGallery);
            }
        }

        private void DeletePromUaProducts(List<XElement> xmlProducts, string sellerId, SyncType importType)
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
            var productIdsToRemove = currentSellerProductIds.Except(xmlProductIds).ToList();
            var productsToRemove = db.Products.Where(entry => productIdsToRemove.Contains(entry.Id)).ToList();
            Parallel.ForEach(productsToRemove, (dbProduct) =>
            {
                dbProduct.AvailabilityState = ProductAvailabilityState.NotInStock;
            });
        }

        public void ImportFromYml(string importTaskId)
        {
            var importTask = db.ExportImports
                .Include(entry => entry.Seller)
                .FirstOrDefault(entry => entry.Id == importTaskId);
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
                CreateAndUpdatePromUaCategories(xmlCategories, importTask.Seller.UrlName, importTask.Seller.Id);
                db.SaveChanges();
                DeleteImportCategories(importTask.Seller, xmlCategories, SyncType.Yml);
                db.SaveChanges();

                var xmlProducts = root.Descendants("offers").First().Elements().ToList();
                AddAndUpdateYmlProducts(xmlProducts, importTask.SellerId);
                db.SaveChanges();
                DeletePromUaProducts(xmlProducts, importTask.SellerId, SyncType.Yml);
                db.SaveChanges();

                //todo:handle exceptions

                importTask.LastUpdateStatus = true;
                importTask.LastUpdateMessage = null;
                importTask.LastSync = DateTime.UtcNow;
                db.Entry(importTask).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                importTask.LastUpdateStatus = false;
                importTask.LastUpdateMessage =
                    "У ході обробки файлу виникли помилки, будь ласка зверніться до служби підтримки";
                importTask.LastSync = DateTime.UtcNow;
                db.Entry(importTask).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void ProcessYmlImportTasks()
        {
            var importTasks =
                db.ExportImports.Include(entry => entry.Seller.MappedCategories).Where(
                    entry => entry.IsActive && entry.IsImport && entry.SyncType == SyncType.Yml).ToList();
            foreach (var importTask in importTasks)
            {
                if (importTask.LastSync.HasValue &&
                    (DateTime.UtcNow - importTask.LastSync.Value).TotalDays < importTask.SyncPeriod)
                {
                    continue;
                }
                ImportFromYml(importTask.Id);
            }
        }

        #endregion

        #region Excel

        public async Task<ProductImportResults> ImportFromExcel(string sellerId, string xlsPath)
        {
            var result = new ProductImportResults();
            var catalog = new LinqToExcel.ExcelQueryFactory(xlsPath);
            var worksheetName = catalog.GetWorksheetNames().FirstOrDefault();
            if (worksheetName == null)
            {
                throw new Exception("Файл імпорту не містить листів");
            }

            var worksheet = catalog.Worksheet(worksheetName);

            var excelProducts =
                (from row in worksheet.AsNoTracking()
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
                         UrlName = row["URL"].Cast<string>() ?? row["Product"].Cast<string>().Translit(),
                         AvailableAmount = row["Stock"].Cast<int>()
                     },
                     Category = categoriesService.GetCategoryByFullName(row["Category"].Cast<string>()),
                     ImagesList = row["Images"].Cast<string>(),
                     CurrencyName = row["Currency"].Cast<string>(),
                     Visible = row["Visible"].Cast<int>()
                 }
                 select item).ToList()
                .Where(entry => entry.Category != null).ToList();

            var currencies = db.Currencies.AsNoTracking().ToList();
            var xlsProducts = new List<Product>();
            excelProducts.ForEach(entry =>
            {
                var product = entry.Product;
                if (product.Description == null)
                {
                    product.Description = product.Name;
                }

                product.CategoryId = entry.Category.Id;
                var curr = currencies.FirstOrDefault(cur => cur.Name == entry.CurrencyName);
                product.CurrencyId = curr == null ? null : curr.Id;
                product.OldPrice = product.OldPrice == default(double) ? (double?)null : product.OldPrice.Value;
                if (entry.Visible > 0)
                {
                    product.AvailabilityState = product.AvailableAmount > 0
                        ? ProductAvailabilityState.Available
                        : ProductAvailabilityState.AlwaysAvailable;
                }
                else
                {
                    product.AvailabilityState = ProductAvailabilityState.NotInStock;
                }

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
                    entry.LastModifiedBy = "ExcelImport";
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
                var xmlProductsToUpdate = xlsProducts.Where(entry => productExternalIdsToUpdate.Contains(entry.ExternalId)).ToList();
                var dbProductsToUpdate = db.Products.Where(entry => productExternalIdsToUpdate.Contains(entry.ExternalId) && entry.SellerId == sellerId);
                productIdsToUpdate = dbProductsToUpdate.ToDictionary(entry => entry.ExternalId, entry => entry.Id);
                Parallel.ForEach(dbProductsToUpdate, (dbProduct) =>
                {
                    var xlsProductToUpdate = xmlProductsToUpdate.First(entry => entry.ExternalId == dbProduct.ExternalId);

                    xlsProductToUpdate.Id = dbProduct.Id;
                    dbProduct.CategoryId = xlsProductToUpdate.CategoryId;
                    dbProduct.Vendor = xlsProductToUpdate.Vendor;
                    dbProduct.Name = xlsProductToUpdate.Name;
                    dbProduct.Title = xlsProductToUpdate.Title;
                    dbProduct.UrlName = xlsProductToUpdate.UrlName;
                    dbProduct.Price = xlsProductToUpdate.Price;
                    dbProduct.OldPrice = xlsProductToUpdate.OldPrice;
                    dbProduct.CurrencyId = xlsProductToUpdate.CurrencyId;
                    dbProduct.AvailabilityState = xlsProductToUpdate.AvailabilityState;
                    dbProduct.AvailableAmount = xlsProductToUpdate.AvailableAmount;
                    dbProduct.ShortDescription = xlsProductToUpdate.ShortDescription;
                    dbProduct.Description = xlsProductToUpdate.Description;
                    dbProduct.LastModified = DateTime.UtcNow;
                    dbProduct.LastModifiedBy = "ExcelImport";
                    dbProduct.SellerId = sellerId;
                });
            }

            var allProductParameters = new List<ProductParameterProduct>();
            var columnNames = catalog.GetColumnNames(worksheetName).ToList();
            var parameterNames = columnNames.GetRange(columnNames.IndexOf("URL") + 1, columnNames.Count - columnNames.IndexOf("URL") - 1);
            foreach (var excelProduct in excelProducts.Where(entry => !productIdsToDelete.Contains(entry.Product.ExternalId)))
            {
                var productParameters = excelProduct.Category.ProductParameters
                    .Where(entry => parameterNames.Contains(entry.Name)).ToList();
                var row = worksheet.FirstOrDefault(entry => entry["SKU"].Cast<string>() == excelProduct.Product.ExternalId);
                foreach (var productParameter in productParameters)
                {
                    allProductParameters.Add(new ProductParameterProduct()
                    {
                        ProductId = excelProduct.Product.Id,
                        ProductParameterId = productParameter.Id,
                        StartValue = row[productParameter.Name].Cast<string>(),
                        StartText = row[productParameter.Name].Cast<string>()
                    });
                }
            }
            db.ProductParameterProducts.RemoveRange(
                db.ProductParameterProducts.Where(entry => productIdsToUpdate.Keys.Contains(entry.ProductId)));
            db.ProductParameterProducts.AddRange(allProductParameters);
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

                foreach (var img in excelProduct.ImagesList.Split(','))
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
                                var ftpImage = new FileInfo(Path.Combine(imagesPath.Name, fileName));
                                if (ftpImage.Exists)
                                {
                                    ftpImage.CopyTo(Path.Combine(destPath, ftpImage.Name), true);
                                }
                            }
                        }
                        ImagesService.AddImage(productId, fileName, imageType);
                    }
                }
            }
            return result;
        }

        #endregion
    }
}
