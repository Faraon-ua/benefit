using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Benefit.Common.Constants;
using Benefit.Common.Extensions;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.XmlModels;
using Benefit.Web.Models.Admin;

namespace Benefit.Services.Domain
{
    public class ProductsService
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private SellerService SellerService = new SellerService();
        public ProductDetailsViewModel GetProductDetails(string urlName, string sellerUrl, string categoryUrl, string userId)
        {
            var product = db.Products
                .Include(entry => entry.Images)
                .Include(entry => entry.Currency)
                .Include(entry => entry.Reviews.Select(rev=>rev.ChildReviews))
                .FirstOrDefault(entry => entry.UrlName == urlName);
            if (product == null) return null;
            product.Price = product.Price*product.Currency.Rate;
            var seller = SellerService.GetSellerWithShippingMethods(sellerUrl);

            product.Seller = seller;
            var categoriesService = new CategoriesService();

            var result = new ProductDetailsViewModel()
            {
                Product = product,
                CategoryUrl = categoryUrl,
                ProductOptions = GetProductOptions(product.Id),
                Breadcrumbs = new BreadCrumbsViewModel()
                {
                    Seller = seller,
                    Categories = categoriesService.GetBreadcrumbs(urlName: categoryUrl),
                    Product = product,
                },
                CanReview = db.Orders.Any(entry => entry.Status == OrderStatus.Finished && entry.UserId == userId && entry.OrderProducts.Any(pr => pr.ProductId == product.Id))
            };

            return result;
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
            var products = db.Products.Where(entry => productIds.Contains(entry.Id));
            Parallel.ForEach(products, (product) =>
            {
                var xmlProductPrice = xmlProductPrices.First(entry => entry.Id == product.Id);
                product.Price = xmlProductPrice.Price;
                productPricesUpdated++;
            });

            db.SaveChanges();
            return productPricesUpdated;
        }

        private void RemoveDuplicates(IEnumerable<Product> collection, string sellerId, string sellerUrlName, bool isIdding)
        {
            if (isIdding)
            {
                //remove repeatings from import comparing to db
                var dbCopies =
                    collection.Where(entry => db.Products.Any(pr => pr.UrlName == entry.UrlName))
                        .Select(entry => entry.Id)
                        .ToList();
                foreach (var pr in collection.Where(entry => dbCopies.Contains(entry.Id)))
                {
                    pr.UrlName = sellerUrlName + "_" + pr.UrlName;
                }
            }
            else
            {
                var dbCopies =
                    collection.Where(entry => db.Products.Any(pr => pr.UrlName == entry.UrlName && pr.SellerId != sellerId))
                        .Select(entry => entry.Id).Distinct()
                        .ToList();
                foreach (var pr in collection.Where(entry => dbCopies.Contains(entry.Id)))
                {
                    pr.UrlName = sellerUrlName + "_" + pr.UrlName;
                }
            }

            var duplicateKeys = collection.GroupBy(x => x.UrlName)
                        .Where(group => group.Count() > 1)
                        .Select(group => group.Key).ToList();
            foreach (var key in duplicateKeys)
            {
                var copies =
                    collection.Where(entry => entry.UrlName == key);
                foreach (var pr in copies)
                {
                    pr.UrlName = pr.UrlName + "|copy" + Guid.NewGuid();
                    pr.Name = pr.Name + "|copy" + Guid.NewGuid();
                }
            }
        }

        public ProductImportResults ProcessImportedProducts(IEnumerable<XmlProduct> xmlProducts, string sellerId, string sellerUrlName)
        {
            var result = new ProductImportResults();
            var dbProductsIds =
                db.Products.Where(entry => entry.SellerId == sellerId).Select(entry => entry.Id).ToList();
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
            if (productIdsToAdd.Any())
            {
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
                                cur => cur.Name == "UAH" && cur.Provider == CurrencyProvider.PrivatBank).Id
                    };
                    productsToAdd.Add(dbProduct);
                });

                RemoveDuplicates(productsToAdd, sellerId, sellerUrlName, true);
                db.Products.AddRange(productsToAdd);
            }

            //update products which are in xml and in db
            var productIdsToUpdate = xmlProductIds.Intersect(dbProductsIds).ToList();
            if (productIdsToUpdate.Any())
            {
                result.ProductsUpdated = productIdsToUpdate.Count;
                var xmlProductsToUpdate = xmlProducts.Where(entry => productIdsToUpdate.Contains(entry.Id)).ToList();
                var dbProductsToUpdate = db.Products.Where(entry => productIdsToUpdate.Contains(entry.Id));
                Parallel.ForEach(dbProductsToUpdate, (dbProduct) =>
                {
                    var xmlProductToUpdate = xmlProductsToUpdate.First(entry => entry.Id == dbProduct.Id);
                    dbProduct.Name = xmlProductToUpdate.Name;
                    dbProduct.UrlName = xmlProductToUpdate.Name.Translit();
                    dbProduct.Description = xmlProductToUpdate.Description;
                    dbProduct.LastModified = DateTime.UtcNow;
                    dbProduct.LastModifiedBy = "1CImport";
                    dbProduct.CategoryId = xmlProductToUpdate.CategoryId;
                    dbProduct.SellerId = sellerId;
                    dbProduct.IsActive = true;
                });
                RemoveDuplicates(dbProductsToUpdate, sellerId, sellerUrlName, false);
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
            imagesService.DeleteAll(product.Images, productId, ImageType.ProductGallery, true, false);

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
