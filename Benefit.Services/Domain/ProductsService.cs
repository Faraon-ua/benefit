using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using Benefit.Common.Constants;
using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.XmlModels;
using Benefit.Web.Models.Admin;

namespace Benefit.Services.Domain
{
    public class ProductsService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ProductImportResults ProcessImportedProducts(IEnumerable<XmlProduct> xmlProducts, IEnumerable<string> dbCategoryIds, string sellerId)
        {
            var result = new ProductImportResults();
            //remove categories
            var dbProductsIds = db.Categories.Where(entry => dbCategoryIds.Contains(entry.Id)).SelectMany(entry => entry.Products).Select(entry => entry.Id);
            var xmlProductIds = xmlProducts.Select(entry => entry.Id);

            //delete products which are not in xml
            var productIdsToDelete = dbProductsIds.Except(xmlProductIds).ToList();
            result.ProductsRemoved = productIdsToDelete.Count;
            db.Products.Where(entry => productIdsToDelete.Contains(entry.Id))
                .ToList()
                .ForEach(entry =>
                {
                    entry.IsActive = false;
                    db.Entry(entry).State = EntityState.Modified;
                });

            var productIdsToAdd = xmlProductIds.Except(dbProductsIds).ToList();
            result.ProductsAdded = productIdsToAdd.Count;
            var sku = (db.Products.Max(pr => (int?)pr.SKU) ?? SettingsService.SkuMinValue) + 1;
            xmlProducts.Where(entry => productIdsToAdd.Contains(entry.Id)).ToList().ForEach(entry =>
            {
                var dbProduct = new Product()
                {
                    Id = entry.Id,
                    Name = entry.Name,
                    UrlName = entry.Name.Translit(),
                    Description = entry.Description,
                    SKU = sku++,
                    Price = entry.Price.GetValueOrDefault(0),
                    Amount = entry.Availability ? null : (int?)0,
                    IsActive = true,
                    LastModified = DateTime.UtcNow,
                    LastModifiedBy = "1CImport",
                    CategoryId = entry.CategoryId,
                    SellerId = sellerId,
                    CurrencyId =
                        db.Currencies.FirstOrDefault(
                            cur => cur.Name == "UAH" && cur.Provider == DomainConstants.DefaultUSDCurrencyProvider).Id
                };
                db.Products.Add(dbProduct);
            });

            var productIdsToUpdate = xmlProductIds.Intersect(dbProductsIds).ToList();
            result.ProductsUpdated = productIdsToUpdate.Count;
            xmlProducts.Where(entry => productIdsToUpdate.Contains(entry.Id)).ToList().ForEach(entry =>
            {
                var dbProduct = db.Products.Find(entry.Id);
                dbProduct.Name = entry.Name;
                dbProduct.UrlName = entry.Name.Translit();
                dbProduct.Description = entry.Description;
                dbProduct.Price = entry.Price.GetValueOrDefault(0);
                dbProduct.Amount = entry.Availability ? null : (int?)0;

                dbProduct.LastModified = DateTime.UtcNow;
                dbProduct.LastModifiedBy = "1CImport";
                dbProduct.CategoryId = entry.CategoryId;
                dbProduct.SellerId = sellerId;

                db.Entry(dbProduct).State = EntityState.Modified;
            });

            db.SaveChanges();
            return result;
        }

        public void Delete(string productId)
        {
            var product =
                db.Products.Include(entry => entry.Images)
                    .Include(entry => entry.ProductOptions)
                    .Include(entry => entry.ProductParameterProducts)
                    .FirstOrDefault(entry => entry.Id == productId);
            if (product == null) return;
            var imagesService = new ImagesService();
            foreach (var image in product.Images)
            {
                imagesService.Delete(image.Id, image.ImageType);
            }

            db.ProductOptions.RemoveRange(product.ProductOptions);
            db.ProductParameterProducts.RemoveRange(product.ProductParameterProducts);
            db.Products.Remove(product);
            db.SaveChanges();
        }
    }
}
