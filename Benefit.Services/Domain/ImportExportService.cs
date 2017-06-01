using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
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

        private void CreateAndUpdatePromUaCategories(List<XElement> xmlCategories, string sellerUrlName, string sellerId, Category parent = null)
        {
            List<XElement> xmlCats = null;
            if (parent == null)
            {
                xmlCats = xmlCategories.Where(entry => entry.Attribute("parentId") == null).ToList();
            }
            else
            {
                xmlCats = xmlCategories.Where(entry => entry.Attribute("parentId") != null && entry.Attribute("parentId").Value == parent.Id).ToList();
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
                        ParentCategoryId = xmlCategory.Attribute("parentId") == null ? null : xmlCategory.Attribute("parentId").Value,
                        IsSellerCategory = true,
                        SellerId = sellerId,
                        Name = catName,
                        UrlName = string.Format("{0}_{1}_{2}", sellerUrlName, parent == null ? string.Empty : parent.Name.Translit(), catName.Translit()),
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
                    dbCategory.Name = catName;
                    dbCategory.UrlName = string.Format("{0}_{1}_{2}", sellerUrlName,
                        parent == null ? string.Empty : parent.Name.Translit(), catName.Translit());

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
            db.Images.RemoveRange(existingImages);

            var currencies = db.Currencies.Where(entry => entry.SellerId == null || entry.SellerId == sellerId).ToList();

            Parallel.ForEach(productIdsToAdd, (productIdToAdd) =>
            {
                var product = dbProducts.FirstOrDefault(entry => entry.Id == productIdToAdd);
                var xmlProduct = xmlProducts.First(entry => entry.Attribute("id").Value == productIdToAdd);

                var name = xmlProduct.Element("name").Value.Replace("\n", "").Replace("\r", "").Trim();
                var shortDescr = xmlProduct.Element("description").Value.Replace("\n", "<br/>");
                var currencyId = xmlProduct.Element("currencyId").Value;

                string descr = null;
                if (xmlProduct.Element("vendor") != null || xmlProduct.Elements("param").Any())
                {
                    var descrBuilder = new StringBuilder();
                    if (xmlProduct.Element("vendor") != null)
                    {
                        descrBuilder.AppendFormat("<tr><td>Виробник</td><td>{0}</td></tr>",
                            xmlProduct.Element("vendor").Value);
                    }
                    foreach (var xmlParam in xmlProduct.Elements("param"))
                    {
                        descrBuilder.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>",
                            xmlParam.Attribute("name").Value, xmlParam.Value);
                    }
                    descr = shortDescr +
                            string.Format("<table><tr><td colspan='2'>Характеристики</td></tr>{0}</table>",
                                descrBuilder);
                }
                product = new Product()
                {
                    Id = xmlProduct.Attribute("id").Value,
                    Name = name,
                    UrlName = name.Translit(),
                    CategoryId = xmlProduct.Element("categoryId").Value,
                    SellerId = sellerId,
                    Description = descr,
                    IsWeightProduct = false,
                    Price = double.Parse(xmlProduct.Element("price").Value),
                    CurrencyId = currencies.First(entry => entry.Name == currencyId).Id,
                    AvailableAmount = xmlProduct.Attribute("available").Value == "true" ? null : (int?)0,
                    IsActive = true,
                    IsImported = true,
                    DoesCountForShipping = true,
                    LastModified = DateTime.UtcNow,
                    LastModifiedBy = "PromUaImport",
                    AltText = name,
                    ShortDescription = name
                };
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
                var shortDescr = xmlProduct.Element("description").Value.Replace("\n", "<br/>");
                string descr = null;
                var currencyId = xmlProduct.Element("currencyId").Value;

                if (xmlProduct.Element("vendor") != null || xmlProduct.Elements("param").Any())
                {
                    var descrBuilder = new StringBuilder();
                    if (xmlProduct.Element("vendor") != null)
                    {
                        descrBuilder.AppendFormat("<tr><td>Виробник</td><td>{0}</td></tr>",
                            xmlProduct.Element("vendor").Value);
                    }
                    foreach (var xmlParam in xmlProduct.Elements("param"))
                    {
                        descrBuilder.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>",
                            xmlParam.Attribute("name").Value, xmlParam.Value);
                    }
                    descr = shortDescr +
                            string.Format("<table><tr><td colspan='2'>Характеристики</td></tr>{0}</table>",
                                descrBuilder);
                }

                product.Name = name;
                product.UrlName = name.Translit();
                product.CategoryId = xmlProduct.Element("categoryId").Value;
                product.Description = descr;
                product.Price = double.Parse(xmlProduct.Element("price").Value);
                product.CurrencyId = currencies.First(entry => entry.Name == currencyId).Id;

                product.AvailableAmount = xmlProduct.Attribute("available").Value == "true" ? null : (int?)0;
                product.LastModified = DateTime.UtcNow;
                product.LastModifiedBy = "PromUaImport";
                product.AltText = name;
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
            });

            foreach (var product in productsToAddList)
            {
                product.SKU = maxSku++;
            }
            db.Products.AddRange(productsToAddList);
            db.Images.AddRange(imagesToAddList);
        }

        private void DeletePromUaProducts(List<XElement> xmlProducts, string sellerId, bool delete)
        {
            var currentSellerProductIds = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported).Select(entry=>entry.Id).ToList();
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
                        return;
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
                        importTask.LastUpdateMessage = "У ході обробки файлу виникли помилки, будь ласка зверніться до служби підтримки";
                        importTask.LastSync = DateTime.UtcNow;
                        db.Entry(importTask).State = EntityState.Modified;
                        db.SaveChanges();
                        return;
                    }
                }
            }
        }
    }
}
