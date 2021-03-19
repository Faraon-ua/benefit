using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.DataAccess.Firebird;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Firebird;
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
    public class FirebirdImportService : BaseImportService
    {
        object lockObj = new object();
        public FirebirdDbContext firebirdDb;
        public FirebirdImportService()
        {
            firebirdDb = new FirebirdDbContext();
        }

        protected override void ProcessImport(ExportImport importTask, ApplicationDbContext db)
        {
            var fbProducts = firebirdDb.GetProducts(importTask.FileUrl);
            var cats = fbProducts.GroupBy(entry => entry.CategoryId).Select(entry => new FirebirdCategory { Id = entry.Key, Name = entry.Select(fb => fb.CategoryName).First() }).ToList();
            CreateAndUpdateFirebirdCategories(cats, importTask.Seller.UrlName, importTask.Seller.Id, db);
            db.SaveChanges();
            DeleteFirebirdCategories(importTask.Seller, cats, db);
            db.SaveChanges();

            AddAndUpdateFirebirdProducts(fbProducts, importTask.SellerId, cats.Select(entry => entry.Id), db);
            db.SaveChanges();
            DeleteFirebirdProducts(fbProducts, importTask.SellerId, db);
            db.SaveChanges();
        }

        private void CreateAndUpdateFirebirdCategories(List<FirebirdCategory> fbCategories, string sellerUrlName,
            string sellerId, ApplicationDbContext db)
        {
            var hasNewContent = false;
            foreach (var fbCategory in fbCategories)
            {
                var dbCategory =
                    db.Categories.FirstOrDefault(entry => entry.ExternalIds == fbCategory.Id && entry.SellerId == sellerId);
                if (dbCategory == null)
                {
                    if (!hasNewContent)
                    {
                        hasNewContent = true;
                    }
                    dbCategory = new Category()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ExternalIds = fbCategory.Id,
                        IsSellerCategory = true,
                        SellerId = sellerId,
                        Name = fbCategory.Name.Truncate(64),
                        UrlName = string.Format("{0}-{1}-{2}", sellerId, fbCategory.Id, fbCategory.Name.Translit()).Truncate(128),
                        Description = fbCategory.Name,
                        MetaDescription = fbCategory.Name,
                        IsActive = true,
                        LastModified = DateTime.UtcNow,
                        LastModifiedBy = "FirebirdImport"
                    };
                    db.Categories.Add(dbCategory);
                }
                else
                {
                    dbCategory.IsActive = true;
                    dbCategory.ExternalIds = fbCategory.Id;
                    dbCategory.Name = fbCategory.Name.Truncate(64);
                    dbCategory.UrlName =
                        string.Format("{0}-{1}-{2}", sellerId, fbCategory.Id, fbCategory.Name.Translit()).Truncate(128);
                    dbCategory.LastModified = DateTime.UtcNow;
                    dbCategory.LastModifiedBy = "FirebirdImport";
                    db.Entry(dbCategory).State = EntityState.Modified;
                }
            }
            if (hasNewContent)
            {
                var importTask = db.ExportImports.FirstOrDefault(entry => entry.SellerId == sellerId);
                importTask.HasNewContent = true;
            }
        }
        public void DeleteFirebirdCategories(Seller seller, IEnumerable<FirebirdCategory> fbCategories, ApplicationDbContext db)
        {
            var CatService = new CategoriesService();
            var currentSellercategoyIds = seller.MappedCategories.Select(entry => entry.ExternalIds).Distinct().ToList();
            List<string> fbCategoryIds = fbCategories.Select(entry => entry.Id).ToList();
            var catIdsToRemove = currentSellercategoyIds.Except(fbCategoryIds).ToList();
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
        private void AddAndUpdateFirebirdProducts(List<FirebirdProduct> fbProducts, string sellerId, IEnumerable<string> categoryIds, ApplicationDbContext db)
        {
            var maxSku = db.Products.Max(entry => entry.SKU) + 1;
            var fbProductIds = fbProducts.Select(entry => entry.Barcode).Where(entry => entry != null).ToList();
            var dbProducts = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported).ToList();
            var dbProductIds = dbProducts.Select(entry => entry.ExternalId).ToList();
            var productIdsToAdd = fbProductIds.Where(entry => !dbProductIds.Contains(entry)).ToList();
            var fbProductsToAdd = fbProducts.Where(entry => productIdsToAdd.Contains(entry.Barcode)).ToList();
            var productIdsToUpdate = fbProductIds.Where(dbProductIds.Contains).ToList();
            var fbCategoryIds = fbProducts.Select(pr => pr.CategoryId).Distinct().ToList();
            var categories = db.Categories
                .Where(entry => fbCategoryIds.Contains(entry.ExternalIds) && entry.SellerId == sellerId).ToList();
            var productsToAddList = new List<Product>();
            var categryIds = categories.Select(pr => pr.Id).ToList();

            Parallel.ForEach(productIdsToAdd, (productIdToAdd) =>
            {
                var fbProduct = fbProducts.First(entry => entry.Barcode == productIdToAdd);
                var name = HttpUtility.HtmlDecode(fbProduct.Name.Replace("\n", "").Replace("\r", "").Trim()).Truncate(256);
                var urlName = name.Translit().Truncate(128);
                var category =
                    categories.FirstOrDefault(entry => entry.ExternalIds == fbProduct.CategoryId);
                if (category == null)
                {
                    return;
                }
                var product = new Product()
                {
                    Id = Guid.NewGuid().ToString(),
                    ExternalId = fbProduct.Barcode,
                    Name = name,
                    Description = name,
                    UrlName = urlName,
                    CategoryId = category.Id,
                    SellerId = sellerId,
                    IsWeightProduct = false,
                    Price = Decimal.ToDouble(fbProduct.Price),
                    AvailableAmount = Decimal.ToInt32(fbProduct.Quantity),
                    IsActive = true,
                    IsImported = true,
                    DoesCountForShipping = true,
                    AltText = name.Truncate(100),
                    ShortDescription = name,
                    ModerationStatus = ModerationStatus.IsModerating,
                    LastModifiedBy = "FirebirdImport",
                    LastModified = DateTime.UtcNow
                };
                lock (lockObj)
                {
                    productsToAddList.Add(product);
                }
            });

            Parallel.ForEach(productIdsToUpdate, (productIdToUpdate) =>
            {
                var product = dbProducts.FirstOrDefault(entry => entry.ExternalId == productIdToUpdate);
                var fbProduct = fbProducts.First(entry => entry.Barcode == productIdToUpdate);
                var category =
                    categories.FirstOrDefault(entry => entry.ExternalIds == fbProduct.CategoryId);
                if (category == null)
                {
                    return;
                }
                product.CategoryId = category.Id;
                product.Price = Decimal.ToDouble(fbProduct.Price);
                product.AvailabilityState = ProductAvailabilityState.Available;
                product.AvailableAmount = Decimal.ToInt32(fbProduct.Quantity);
                product.LastModified = DateTime.UtcNow;
                product.LastModifiedBy = "FirebirdImport";
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
        private void DeleteFirebirdProducts(List<FirebirdProduct> fbProducts, string sellerId, ApplicationDbContext db)
        {
            var currentSellerProductIds = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported)
                .Select(entry => entry.ExternalId).Distinct().ToList();
            List<string> fbProductIds = null;
            fbProductIds = fbProducts.Select(entry => entry.Barcode).Distinct().ToList();
            var productIdsToRemove = currentSellerProductIds.Except(fbProductIds).Where(entry => entry != null).ToList();
            var productsToRemove = db.Products.Where(entry => entry.SellerId == sellerId && productIdsToRemove.Contains(entry.ExternalId)).ToList();
            Parallel.ForEach(productsToRemove, (dbProduct) =>
            {
                dbProduct.AvailabilityState = ProductAvailabilityState.NotInStock;
            });
        }
    }
}
