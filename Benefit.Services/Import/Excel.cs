using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Excel;
using Benefit.Services.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Benefit.Services.Import
{
    public class ExcelImportService : BaseImportService
    {
        private CategoriesService categoriesService = new CategoriesService();
        private ImagesService ImagesService = new ImagesService();

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
        protected override void ProcessImport(ExportImport importTask, ApplicationDbContext db)
        {
            var sellerId = importTask.SellerId;
            var ftpDirectory = new DirectoryInfo(originalDirectory).FullName;
            var sellerPath = Path.Combine(ftpDirectory, "FTP", "LocalUser", importTask.Seller.UrlName);
            var importFile = new DirectoryInfo(sellerPath).GetFiles("import.xls", SearchOption.AllDirectories).FirstOrDefault();

            if (importFile == null || importFile.Length == 0)
            {
                importTask.LastUpdateStatus = false;
                importTask.LastSync = DateTime.UtcNow;
                importTask.LastUpdateMessage = "Файл import.xml не знайдено";
                db.SaveChanges();
                return;
            }
            var catalog = new LinqToExcel.ExcelQueryFactory(importFile.FullName);
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
            ProcessExcelCategories(allCats, allDbCats, importTask.SellerId, null, db);
            var alldDbNames = allDbCats.Select(entry => entry.Name).ToList();
            var inActiveCategories = db.Categories
                .Where(entry => entry.SellerId == importTask.SellerId && !alldDbNames.Contains(entry.Name)).ToList();
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
            var path = new DirectoryInfo(importFile.FullName);
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
        }
    }
}
