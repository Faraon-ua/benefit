﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.XmlModels;

namespace Benefit.Services.Domain
{
    public class ProductsService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public int AddToFavorites(string userId, string productId)
        {
            if (db.Favorites.FirstOrDefault(entry => entry.UserId == userId && entry.ProductId == productId) != null) return 0;
            var favorite = new Favorite()
            {
                UserId = userId,
                ProductId = productId
            };
            db.Favorites.Add(favorite);
            db.SaveChanges();
            return db.Favorites.Count(entry => entry.UserId == userId);
        }

        public int RemoveFromFavorites(string userId, string productId)
        {
            var favorite = db.Favorites.FirstOrDefault(entry => entry.UserId == userId && entry.ProductId == productId);
            if (favorite == null) return 0;
            db.Favorites.Remove(favorite);
            db.SaveChanges();
            return db.Favorites.Count(entry => entry.UserId == userId);
        }

        public List<Product> GetFavorites(string userId)
        {
            return db.Favorites.Include(entry=>entry.Product).Where(entry => entry.UserId == userId).Select(entry=>entry.Product).ToList();
        }

        public ProductDetailsViewModel GetProductDetails(IEnumerable<Category> cachedCats, string urlName, string userId)
        {
            var delimeterPos = urlName.LastIndexOf("-") + 1;
            var sku = urlName.Substring(delimeterPos, urlName.Length - delimeterPos);
            var product = db.Products
                .Include(entry => entry.Category.MappedParentCategory)
                .Include(entry => entry.Seller.ShippingMethods.Select(addr => addr.Region))
                .Include(entry => entry.Seller.Addresses.Select(addr => addr.Region))
                .Include(entry => entry.Images)
                .Include(entry => entry.Currency)
                .Include(entry => entry.ProductParameterProducts.Select(pr => pr.ProductParameter))
                .Include(entry => entry.Reviews.Select(rev => rev.ChildReviews))
                .FirstOrDefault(entry => entry.SKU.ToString() == sku);
            if (product == null) return null;
            if (!string.IsNullOrEmpty(product.Vendor))
            {
                product.ProductParameterProducts.Add(new ProductParameterProduct()
                {
                    ProductParameter = new ProductParameter()
                    {
                        Name = "Виробник"
                    },
                    StartValue = product.Vendor
                });
            }

            if (!string.IsNullOrEmpty(product.OriginCountry))
            {
                product.ProductParameterProducts.Add(new ProductParameterProduct()
                {
                    ProductParameter = new ProductParameter()
                    {
                        Name = "Країна виробник"
                    },
                    StartValue = product.OriginCountry
                });
            }

            if (!product.Seller.IsActive || !product.Category.IsActive)
            {
                product.AvailabilityState = ProductAvailabilityState.NotInStock;
            }
            if (product.Currency != null)
            {
                product.Price = product.Price * product.Currency.Rate;
            }

            var categoriesService = new CategoriesService();
            var result = new ProductDetailsViewModel()
            {
                Product = product,
                ProductOptions = GetProductOptions(product.Id),
                Breadcrumbs = new BreadCrumbsViewModel()
                {
                    Categories = categoriesService.GetBreadcrumbs(cachedCats, urlName: product.Category.UrlName),
                    Product = product,
                },
                CanReview = db.Orders.Any(entry => entry.Status == OrderStatus.Finished && entry.UserId == userId && entry.OrderProducts.Any(pr => pr.ProductId == product.Id))
            };
            var sellerCategory = db.SellerCategories.FirstOrDefault(entry =>
                                     entry.CategoryId == product.CategoryId && entry.SellerId == product.SellerId) ??
                                 db.SellerCategories.FirstOrDefault(entry =>
                                     entry.CategoryId == product.Category.MappedParentCategoryId &&
                                     entry.SellerId == product.SellerId);
            if (sellerCategory != null && sellerCategory.CustomDiscount.HasValue)
            {
                result.DiscountPercent = sellerCategory.CustomDiscount.Value;
            }
            else
            {
                result.DiscountPercent = product.Seller.UserDiscount;
            }

            var category = product.Category.IsSellerCategory ? product.Category.MappedParentCategory : product.Category;
            var catsIds = category.MappedCategories.Select(entry => entry.Id).ToList();
            catsIds.Add(category.Id);
            result.RelatedProducts = db.Products
                .Include(entry => entry.Seller)
                .Include(entry => entry.Currency)
                .Include(entry => entry.Images)
                .Where(entry =>
                    entry.IsActive && entry.Seller.IsActive &&
                    entry.Id != product.Id &&
                    entry.AvailabilityState != ProductAvailabilityState.NotInStock && entry.Images.Any() &&
                    catsIds.Contains(entry.CategoryId)).OrderBy(entry => Guid.NewGuid()).Take(5)
                .ToList();
            foreach (var relatedProduct in result.RelatedProducts)
            {
                if (relatedProduct.Currency != null)
                {
                    relatedProduct.Price = relatedProduct.Price * relatedProduct.Currency.Rate;
                }
            }
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
            productOptions.ForEach(entry => entry.ChildProductOptions = entry.ChildProductOptions.OrderBy(po => po.Order).ToList());
            return productOptions.OrderBy(entry => entry.Order).ToList();
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
                product.AvailableAmount = (int)xmlProductPrice.Amount;
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
            var productparameter = db.ProductParameters.FirstOrDefault(entry => entry.Id == id);
            //remove self values and product relations
            db.ProductParameterValues.RemoveRange(productparameter.ProductParameterValues);
            db.ProductParameterProducts.RemoveRange(productparameter.ProductParameterProducts);

            db.ProductParameters.Remove(productparameter);
            db.SaveChanges();
        }
    }
}

