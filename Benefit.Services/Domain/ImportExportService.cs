﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Web.Models.Admin;
using NLog;

namespace Benefit.Services.Domain
{
    public class ImportExportService
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private CategoriesService categoriesService = new CategoriesService();
        private ProductsService productsService = new ProductsService();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        Object lockObj = new Object();

        private void CreateAndUpdatePromUaCategories(List<XElement> xmlCategories, string sellerUrlName,
            string sellerId, Category parent = null)
        {
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
                        NavigationType = CategoryNavigationType.SellersAndProducts.ToString(),
                        IsActive = true,
                        LastModified = DateTime.UtcNow,
                        LastModifiedBy = "ImportFromPromua"
                    };
                    db.Categories.Add(dbCategory);
                }
                else
                {
                    dbCategory.Name = catName.Truncate(64);
                    dbCategory.UrlName = string.Format("{0}_{1}", catId, catName.Translit()).Truncate(128);

                    dbCategory.ParentCategoryId = xmlCategory.Attribute("parentId") == null
                        ? null
                        : xmlCategory.Attribute("parentId").Value;
                    db.Entry(dbCategory).State = EntityState.Modified;
                }

                CreateAndUpdatePromUaCategories(xmlCategories, sellerUrlName, sellerId, dbCategory);
            }
        }

        private void DeletePromUaCategories(Seller seller, IEnumerable<XElement> xmlCategories, bool delete)
        {
            var currentSellercategoyIds = seller.MappedCategories.Select(entry => entry.Id).ToList();
            var xmlCategoryIds = xmlCategories.Select(entry => entry.Attribute("id").Value).ToList();
            var catIdsToRemove = currentSellercategoyIds.Except(xmlCategoryIds).ToList();
            foreach (var catId in catIdsToRemove)
            {
                if (delete)
                {
                    categoriesService.Delete(catId);
                }
                else
                {
                    var dbCategory = db.Categories.Find(catId);
                    dbCategory.IsActive = false;
                    db.Entry(dbCategory).State = EntityState.Modified;
                }
            }
        }

        private void AddAndUpdatePromUaProducts(List<XElement> xmlProducts, string sellerId)
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
                        AddedBy = "PromUaImport",
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
                var name = xmlProduct.Element("name").Value.Replace("\n", "").Replace("\r", "").Trim();
                var descr = xmlProduct.Element("description").GetValueOrDefault(string.Empty).Replace("\n", "<br/>");
                var currencyId = xmlProduct.Element("currencyId").Value;
                var urlName = name.Translit();
                var product = new Product()
                {
                    Id = xmlProduct.Attribute("id").Value,
                    Name = name,
                    UrlName = urlName.Truncate(128),
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
                productParameterProductsToAdd.AddRange(productParams);

                lock (lockObj)
                {
                    productsToAddList.Add(product);
                }

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
            });

            Parallel.ForEach(productIdsToUpdate, (productIdToUpdate) =>
            {
                var product = dbProducts.FirstOrDefault(entry => entry.Id == productIdToUpdate);
                var xmlProduct = xmlProducts.First(entry => entry.Attribute("id").Value == productIdToUpdate);

                var name = xmlProduct.Element("name").Value;
                var descr = xmlProduct.Element("description").GetValueOrDefault(string.Empty).Replace("\n", "<br/>");
                var currencyId = xmlProduct.Element("currencyId").Value;

                product.Name = name;
                product.UrlName = string.Format("{0}_{1}", product.SKU, name.Translit()).Truncate(128);
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
        }

        private void DeletePromUaProducts(List<XElement> xmlProducts, string sellerId, bool delete)
        {
            var currentSellerProductIds = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported)
                .Select(entry => entry.Id).ToList();
            var xmlProductIds = xmlProducts.Select(entry => entry.Attribute("id").Value).ToList();
            var productIdsToRemove = currentSellerProductIds.Except(xmlProductIds).ToList();
            foreach (var prodId in productIdsToRemove)
            {
                if (delete)
                {
                    productsService.Delete(prodId);
                }
                else
                {
                    var dbProduct = db.Products.Find(prodId);
                    dbProduct.IsActive = false;
                    db.Entry(dbProduct).State = EntityState.Modified;
                }
            }
        }

        public void ImportFromPromua()
        {
            var importTasks =
                db.ExportImports.Include(entry => entry.Seller.MappedCategories).Where(
                    entry => entry.IsActive && entry.IsImport && entry.SyncType == SyncType.Promua).ToList();
            foreach (var importTask in importTasks)
            {
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
                    continue;
                }

                try
                {
                    var root = xml.Element("yml_catalog").Element("shop");
                    var xmlCategories = root.Descendants("categories").First().Elements().ToList();
                    CreateAndUpdatePromUaCategories(xmlCategories, importTask.Seller.UrlName, importTask.Seller.Id);
                    db.SaveChanges();
                    DeletePromUaCategories(importTask.Seller, xmlCategories, importTask.RemoveProducts);
                    db.SaveChanges();

                    var xmlProducts = root.Descendants("offers").First().Elements().ToList();
                    AddAndUpdatePromUaProducts(xmlProducts, importTask.SellerId);
                    db.SaveChanges();
                    DeletePromUaProducts(xmlProducts, importTask.SellerId, importTask.RemoveProducts);
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
                    return;
                }
            }
        }

        public async Task<ProductImportResults> ImportFromExcel(string sellerId, string xlsPath)
        {
            var result = new ProductImportResults();
            var catalog = new LinqToExcel.ExcelQueryFactory(xlsPath);
            var worksheetName = catalog.GetWorksheetNames().FirstOrDefault();
            if (worksheetName == null) throw new Exception("Файл імпорту не містить листів");
            var worksheet = catalog.Worksheet(worksheetName);

            var xlsProducts =
                (from row in worksheet.AsNoTracking()
                 let item = new Product()
                 {
                     ExternalId = row["SKU"].Cast<string>(),
                     Category = categoriesService.GetCategoryByFullName(row["Category"].Cast<string>()),
                     Vendor = row["Brand"].Cast<string>(),
                     Name = row["Product"].Cast<string>(),
                     Price = row["Price"].Cast<double>(),
                     OldPrice = row["Old price"].Cast<double>(),
                     CurrencyId = row["Currency"].Cast<string>(),
                     IsFeatured = row["Featured"].Cast<bool>(),
                     Title = row["Meta title"].Cast<string>(),
                     Description = row["Description"].Cast<string>(),
                     ShortDescription = row["Meta description"].Cast<string>(),
                     UrlName = row["URL"].Cast<string>(),
                     AvailabilityState = row["Visible"].Cast<bool>() ? ProductAvailabilityState.Available : ProductAvailabilityState.NotInStock,
                     AvailableAmount = row["Stock"].Cast<int>(),
                     Images = row["Images"].Cast<string>().Split(',').Select(entry=>new Image()
                     {
                         Id = Guid.NewGuid().ToString(),
                         ImageUrl = entry
                     }).ToList()
                 }
                 select item).ToList()
                .Where(entry => entry.Category != null).ToList();

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
            List<string> productIdsToAdd = null, productIdsToUpdate = null;
            if (productExternalIdsToAdd.Any())
            {
                result.ProductsAdded = productExternalIdsToAdd.Count;
                var sku = (db.Products.Max(pr => (int?)pr.SKU) ?? SettingsService.SkuMinValue) + 1;
                var productsToAdd = new List<Product>();
                xlsProducts.Where(entry => productExternalIdsToAdd.Contains(entry.ExternalId)).ToList().ForEach(entry =>
                {
                    entry.Id = Guid.NewGuid().ToString();
                    entry.CategoryId = entry.Category.Id;
                    entry.CurrencyId = db.Currencies.AsNoTracking().First(cur => cur.Name == entry.CurrencyId).Id;
                    entry.SKU = sku++;
                    entry.LastModified = DateTime.UtcNow;
                    entry.LastModifiedBy = "ExcelImport";
                    entry.SellerId = sellerId;
                    productsToAdd.Add(entry);
                });
                productIdsToAdd = productsToAdd.Select(entry => entry.Id).ToList();
                db.Products.AddRange(productsToAdd);
            }

            var productExternalIdsToUpdate = xlsProductIds.Intersect(dbProductsIds).ToList();
            if (productExternalIdsToUpdate.Any())
            {
                result.ProductsUpdated = productExternalIdsToUpdate.Count;
                var xmlProductsToUpdate = xlsProducts.Where(entry => productExternalIdsToUpdate.Contains(entry.Id)).ToList();
                var dbProductsToUpdate = db.Products.Where(entry => productExternalIdsToUpdate.Contains(entry.Id));
                productIdsToUpdate = dbProductsToUpdate.Select(entry => entry.Id).ToList();
                Parallel.ForEach(dbProductsToUpdate, (dbProduct) =>
                {
                    var xlsProductToUpdate = xmlProductsToUpdate.First(entry => entry.Id == dbProduct.Id);

                    dbProduct.CategoryId = xlsProductToUpdate.Category.Id;
                    dbProduct.Vendor = xlsProductToUpdate.Vendor;
                    dbProduct.Name = xlsProductToUpdate.Name;
                    dbProduct.Title = xlsProductToUpdate.Title;
                    dbProduct.UrlName = xlsProductToUpdate.UrlName;
                    dbProduct.Price = xlsProductToUpdate.Price;
                    dbProduct.OldPrice = xlsProductToUpdate.OldPrice;
                    dbProduct.CurrencyId = db.Currencies.First(cur => cur.Name == dbProduct.Id).Id;
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
            foreach (var xlsProduct in xlsProducts.Where(entry => !productIdsToDelete.Contains(entry.ExternalId)))
            {
                var productParameters = xlsProduct.Category.ProductParameters
                    .Where(entry => parameterNames.Contains(entry.Name)).ToList();
                var row = worksheet.FirstOrDefault(entry => entry["SKU"].Cast<string>() == xlsProduct.ExternalId);
                foreach (var productParameter in productParameters)
                {
                    allProductParameters.Add(new ProductParameterProduct()
                    {
                        ProductId = xlsProduct.Id,
                        ProductParameterId = productParameter.Id,
                        StartValue = row[productParameter.Name].Cast<string>(),
                        StartText = row[productParameter.Name].Cast<string>()
                    });
                }
            }
            db.ProductParameterProducts.RemoveRange(
                db.ProductParameterProducts.Where(entry => productIdsToUpdate.Contains(entry.ProductId)));
            db.ProductParameterProducts.AddRange(allProductParameters);
            db.SaveChanges();

            //images
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var path = new DirectoryInfo(xlsPath);
            var imagesPath = path.GetDirectories("images", SearchOption.AllDirectories)
                    .FirstOrDefault();
            if (imagesPath.Exists)
            {
                var imageType = ImageType.ProductGallery;
                //foreach (var xlsProduct in xlsProducts.Where(entry => entry.Images.Any()))
                //{
                //    var destPath = Path.Combine(originalDirectory, "Images", imageType.ToString(), xlsProduct.Id);
                //    var isExists = Directory.Exists(destPath);
                //    if (!isExists)
                //        Directory.CreateDirectory(destPath);

                //    var ftpImage = new FileInfo(Path.Combine(ftpImagesPath, xmlProduct.Image));
                //    if (ftpImage.Exists)
                //    {
                //        ImagesService.DeleteAll(
                //            db.Images.Where(entry => entry.ProductId == xmlProduct.Id).ToList(), xmlProduct.Id,
                //            imageType);
                //        ftpImage.CopyTo(Path.Combine(destPath, ftpImage.Name), true);
                //        ImagesService.AddImage(xmlProduct.Id, ftpImage.Name, imageType);
                //    }
                //}
            }


            return result;
        }
    }
}
