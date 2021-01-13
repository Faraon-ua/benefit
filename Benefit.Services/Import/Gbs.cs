using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Benefit.Services.Import
{
    public class GbsImportService
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private ImportExportService ImportExportService = new ImportExportService();
        object lockObj = new object();

        public void Import(string importTaskId)
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
                _logger.Info(string.Format("Gbs Import started at {0} {1}", importTask.Seller.Id, importTask.Seller.UrlName));

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
                    var root = xml.Element("gbsmarket");
                    var xmlCategories = root.Descendants("GoodsCategories").ToList();
                    var xmlCategoryIds = xmlCategories.Select(entry => entry.Element("Id").Value).ToList();
                    CreateAndUpdateGbsCategories(xmlCategories, importTask.Seller.UrlName, importTask.Seller.Id, db);
                    db.SaveChanges();
                    ImportExportService.DeleteImportCategories(importTask.Seller, xmlCategories, SyncType.Gbs, db);
                    db.SaveChanges();

                    var xmlProducts = root.Descendants("goods").ToList();
                    AddAndUpdateGbsProducts(xmlProducts, importTask.SellerId, xmlCategoryIds, db);
                    db.SaveChanges();
                    DeleteGbsProducts(xmlProducts, importTask.SellerId, db);
                    db.SaveChanges();

                    //todo:handle exceptions

                    importTask.LastUpdateStatus = true;
                    importTask.LastUpdateMessage = null;
                    _logger.Info(string.Format("GBS Import success at {0} {1}", importTask.Seller.Id, importTask.Seller.Name));
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
                    _logger.Info(string.Format("GBS results saved {0} {1}", importTask.Seller.Id, importTask.Seller.Name));
                }
            }
        }

        private void CreateAndUpdateGbsCategories(List<XElement> xmlCategories, string sellerUrlName,
            string sellerId, ApplicationDbContext db, Category parent = null)
        {
            var hasNewContent = false;
            List<XElement> xmlCats = null;
            if (parent == null)
            {
                xmlCats = xmlCategories.Where(entry => entry.Element("id_parent").Value == null || (entry.Element("id_parent").Value != null && entry.Element("id_parent").Value == "-1")).ToList();
            }
            else
            {
                xmlCats = xmlCategories.Where(entry =>
                    entry.Element("id_parent").Value != null && entry.Element("id_parent").Value == parent.ExternalIds).ToList();
            }

            foreach (var xmlCategory in xmlCats)
            {
                var catId = xmlCategory.Element("Id").Value;
                var catName = xmlCategory.Element("Name").Value.Replace("\n", "").Replace("\r", "").Trim();
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
                        LastModifiedBy = "ImportFromGbs"
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

                CreateAndUpdateGbsCategories(xmlCategories, sellerUrlName, sellerId, db, dbCategory);
            }
            if (hasNewContent)
            {
                var importTask = db.ExportImports.FirstOrDefault(entry => entry.SellerId == sellerId);
                importTask.HasNewContent = true;
            }
        }

        private void AddAndUpdateGbsProducts(List<XElement> xmlProducts, string sellerId, IEnumerable<string> categoryIds, ApplicationDbContext db)
        {
            var maxSku = db.Products.Max(entry => entry.SKU) + 1;
            var xmlProductIds = xmlProducts.Select(entry => entry.Element("barcode").Value).ToList();
            var dbProducts = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported).ToList();
            var dbProductIds = dbProducts.Select(entry => entry.ExternalId).ToList();
            var productIdsToAdd = xmlProductIds.Where(entry => !dbProductIds.Contains(entry)).ToList();
            var xmlProductsToAdd = xmlProducts.Where(entry => productIdsToAdd.Contains(entry.Element("barcode").Value)).ToList();
            var productIdsToUpdate = xmlProductIds.Where(dbProductIds.Contains).ToList();
            var xmlCategoryIds = xmlProducts.Select(pr => pr.Element("group_id").Value).Distinct().ToList();
            var categories = db.Categories
                .Where(entry => xmlCategoryIds.Contains(entry.ExternalIds) && entry.SellerId == sellerId).ToList();
            var productsToAddList = new List<Product>();
            var categryIds = categories.Select(pr => pr.Id).ToList();

            Parallel.ForEach(productIdsToAdd, (productIdToAdd) =>
            {
                var xmlProduct = xmlProducts.First(entry => entry.Element("barcode").Value == productIdToAdd);
                var name = HttpUtility.HtmlDecode(xmlProduct.Element("name").Value.Replace("\n", "").Replace("\r", "").Trim()).Truncate(256);
                var urlName = name.Translit().Truncate(128).Replace(" ", "-");
                var category =
                    categories.FirstOrDefault(entry => entry.ExternalIds == xmlProduct.Element("group_id").Value);
                if (category == null)
                {
                    return;
                }
                var product = new Product()
                {
                    Id = Guid.NewGuid().ToString(),
                    ExternalId = xmlProduct.Element("barcode").Value,
                    Name = name,
                    Description = name,
                    UrlName = urlName,
                    CategoryId = category.Id,
                    SellerId = sellerId,
                    IsWeightProduct = false,
                    Price = double.Parse(xmlProduct.Element("price").Value),
                    AvailableAmount = int.Parse(xmlProduct.Element("stock").Value),
                    IsActive = true,
                    IsImported = true,
                    DoesCountForShipping = true,
                    LastModified = DateTime.UtcNow,
                    AltText = name.Truncate(100),
                    ShortDescription = name,
                    ModerationStatus = ModerationStatus.IsModerating
                };
                lock (lockObj)
                {
                    productsToAddList.Add(product);
                }
            });

            Parallel.ForEach(productIdsToUpdate, (productIdToUpdate) =>
            {
                var product = dbProducts.FirstOrDefault(entry => entry.ExternalId == productIdToUpdate);
                var xmlProduct = xmlProducts.First(entry => entry.Element("barcode").Value == productIdToUpdate);
                var category =
                    categories.FirstOrDefault(entry => entry.ExternalIds == xmlProduct.Element("group_id").Value);
                if (category == null)
                {
                    return;
                }
                product.Price = double.Parse(xmlProduct.Element("price").Value, CultureInfo.InvariantCulture);
                product.AvailabilityState = ProductAvailabilityState.Available;
                product.AvailableAmount = int.Parse(xmlProduct.Element("stock").Value);
                product.LastModified = DateTime.UtcNow;
            });

            foreach (var product in productsToAddList)
            {
                product.SKU = maxSku;
                product.UrlName = product.UrlName.Insert(0, maxSku++ + "-").Truncate(128);
            }
            productsToAddList = productsToAddList.Distinct(new ProductComparer()).ToList();
            db.InsertIntoMembers(productsToAddList);
            db.SaveChanges();
        }
        private void DeleteGbsProducts(List<XElement> xmlProducts, string sellerId, ApplicationDbContext db)
        {
            var currentSellerProductIds = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported)
                .Select(entry => entry.ExternalId).Distinct().ToList();
            List<string> xmlProductIds = null;
            xmlProductIds = xmlProducts.Select(entry => entry.Element("barcode").Value).Distinct().ToList();
            var productIdsToRemove = currentSellerProductIds.Except(xmlProductIds).Where(entry => entry != null).ToList();
            var productsToRemove = db.Products.Where(entry => entry.SellerId == sellerId && productIdsToRemove.Contains(entry.ExternalId)).ToList();
            Parallel.ForEach(productsToRemove, (dbProduct) =>
            {
                dbProduct.AvailabilityState = ProductAvailabilityState.NotInStock;
            });
        }
    }
}
