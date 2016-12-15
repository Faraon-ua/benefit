using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

        public Product GetProduct(string urlName)
        {
            var product = db.Products.Include(entry => entry.Images).FirstOrDefault(entry => entry.UrlName == urlName);
            if (product == null) return null;
            return product;
        }

        public List<ProductOption> GetProductOptions(string productId)
        {
            var product = db.Products.Include(entry => entry.Category).Include(entry => entry.Seller).FirstOrDefault(entry => entry.Id == productId);
            var productOptions = db.ProductOptions.Include(entry => entry.ChildProductOptions).Where(entry => entry.ParentProductOptionId == null && entry.ProductId == productId).ToList();
            var categoryProductOptions = db.ProductOptions.Include(entry => entry.ChildProductOptions).Where(
                entry =>
                    entry.CategoryId == product.CategoryId && entry.SellerId == product.SellerId &&
                    entry.ParentProductOptionId == null).ToList();
            productOptions.InsertRange(0, categoryProductOptions);
            return productOptions;
        }

        public int ProcessImportedProductPrices(IEnumerable<XmlProductPrice> xmlProductPrices)
        {
            int productPricesUpdated = 0;
            var productIds = xmlProductPrices.Select(entry => entry.Id).ToList();
            var products = db.Products.Where(entry => productIds.Contains(entry.Id)).ToList();
            foreach (var product in products)
            {
                var xmlProductPrice = xmlProductPrices.FirstOrDefault(entry => entry.Id == product.Id);
                product.Price = xmlProductPrice.Price;
                db.Entry(product).State = EntityState.Modified;
                productPricesUpdated++;
            }
            db.SaveChanges();
            return productPricesUpdated;
        }

        public ProductImportResults ProcessImportedProducts(IEnumerable<XmlProduct> xmlProducts, IEnumerable<string> dbCategoryIds, string sellerId)
        {
            var result = new ProductImportResults();
            //remove categories
            var dbProductsIds = db.Categories.Where(entry => dbCategoryIds.Contains(entry.Id)).SelectMany(entry => entry.Products).Where(entry => entry.SellerId == sellerId).Select(entry => entry.Id).ToList();
            var xmlProductIds = xmlProducts.Select(entry => entry.Id).ToList();

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

            //add products which are in xml and not in db
            var productIdsToAdd = xmlProductIds.Except(dbProductsIds).ToList();
            result.ProductsAdded = productIdsToAdd.Count;
            var sku = (db.Products.Max(pr => (int?)pr.SKU) ?? SettingsService.SkuMinValue) + 1;
            var productsToAdd = new List<Product>();
            xmlProducts.Where(entry => productIdsToAdd.Contains(entry.Id)).ToList().ForEach(entry =>
            {
                var dbProduct = new Product()
                {
                    Id = entry.Id,
                    Name = entry.Name,
                    UrlName = entry.Name.Translit(),
                    Description = entry.Description,
                    SKU = sku++,
                    IsActive = true,
                    LastModified = DateTime.UtcNow,
                    LastModifiedBy = "1CCommerceMLImport",
                    CategoryId = entry.CategoryId,
                    SellerId = sellerId,
                    CurrencyId =
                        db.Currencies.FirstOrDefault(
                            cur => cur.Name == "UAH" && cur.Provider == DomainConstants.DefaultUSDCurrencyProvider).Id
                };
                productsToAdd.Add(dbProduct);
            });
            foreach (var productToAdd in productsToAdd)
            {
                var copies =
                    productsToAdd.Where(entry => entry.Name == productToAdd.Name && entry.Id != productToAdd.Id);
                for (int i = 0; i < copies.Count(); i++)
                {
                    var pr = copies.ElementAt(i);
                    pr.UrlName = pr.UrlName + "copy" + i;
                    pr.Name = pr.Name + "[copy]" + i;
                }
            }
            db.Products.AddRange(productsToAdd);

            //update products which are in xml and in db
            var productIdsToUpdate = xmlProductIds.Intersect(dbProductsIds).ToList();
            result.ProductsUpdated = productIdsToUpdate.Count;
            var xmlProductsToUpdate = xmlProducts.Where(entry => productIdsToUpdate.Contains(entry.Id)).ToList();
            foreach (var xmlProductToUpdate in xmlProductsToUpdate)
            {
                var dbProduct = db.Products.Find(xmlProductToUpdate.Id);
                dbProduct.Name = xmlProductToUpdate.Name;
                dbProduct.UrlName = xmlProductToUpdate.Name.Translit();
                dbProduct.Description = xmlProductToUpdate.Description;
                dbProduct.LastModified = DateTime.UtcNow;
                dbProduct.LastModifiedBy = "1CImport";
                dbProduct.CategoryId = xmlProductToUpdate.CategoryId;
                dbProduct.SellerId = sellerId;
                dbProduct.IsActive = true;

                db.Entry(dbProduct).State = EntityState.Modified;
            }

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
            imagesService.DeleteAll(product.Images, productId, ImageType.ProductGallery);

            db.ProductOptions.RemoveRange(product.ProductOptions);
            db.ProductParameterProducts.RemoveRange(product.ProductParameterProducts);
            db.Products.Remove(product);
            db.SaveChanges();
        }

        public void DeleteProductParameter(string id)
        {
            var productparameter = db.ProductParameters.Include(entry => entry.ChildProductParameters).Include(entry => entry.ChildProductParameters.Select(child => child.ProductParameterValues)).FirstOrDefault(entry => entry.Id == id);
            //remove self values and product relations
            db.ProductParameterValues.RemoveRange(productparameter.ProductParameterValues);
            db.ProductParameterProducts.RemoveRange(productparameter.ProductParameterProducts);

            //remove childs's values and product relations
            db.ProductParameterValues.RemoveRange(productparameter.ChildProductParameters.SelectMany(entry => entry.ProductParameterValues));
            db.ProductParameterProducts.RemoveRange(productparameter.ChildProductParameters.SelectMany(entry => entry.ProductParameterProducts));
            //remove childs
            db.ProductParameters.RemoveRange(productparameter.ChildProductParameters);

            db.ProductParameters.Remove(productparameter);
            db.SaveChanges();
        }
    }
}
