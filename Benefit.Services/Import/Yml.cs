using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Benefit.Services.Import
{
    public class YmlImportService : BaseImportService
    {
        private ImagesService ImagesService = new ImagesService();
        object lockObj = new object();
        protected override void ProcessImport(ExportImport importTask)
        {
            using (var db = new ApplicationDbContext())
            {
                XDocument xml = null;
                xml = XDocument.Load(importTask.FileUrl);

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
                DeleteYmlProducts(xmlProducts, importTask.SellerId, db);
                db.SaveChanges();
            }
        }

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
                    var currencyId = xmlProduct.Element("currencyId") == null ? "UAH" : xmlProduct.Element("currencyId").Value;
                    var urlName = name.Translit().Truncate(128).Replace(" ", "-");
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
                        Price = double.Parse(xmlProduct.Element("price").Value, CultureInfo.InvariantCulture),
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
                    product.Price = double.Parse(xmlProduct.Element("price").Value, CultureInfo.InvariantCulture);
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
                    product.UrlName = product.UrlName.Insert(0, maxSku++ + "-").Truncate(128);
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

        private void DeleteYmlProducts(List<XElement> xmlProducts, string sellerId, ApplicationDbContext db)
        {
            var currentSellerProductIds = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported)
                .Select(entry => entry.Id).ToList();
            List<string> xmlProductIds = null;
            xmlProductIds = xmlProducts.Select(entry => entry.Attribute("id").Value).ToList();
            var productIdsToRemove = currentSellerProductIds.Except(xmlProductIds).Where(entry => entry != null).ToList();
            var productsToRemove = db.Products.Where(entry => entry.SellerId == sellerId && productIdsToRemove.Contains(entry.Id)).ToList();
            Parallel.ForEach(productsToRemove, (dbProduct) =>
            {
                dbProduct.AvailabilityState = ProductAvailabilityState.NotInStock;
            });
        }
    }
}
