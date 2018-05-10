using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using System.Data.Entity;
using System.Web.Caching;
using Benefit.DataTransfer.ViewModels.Base;
using Benefit.DataTransfer.ViewModels.NavigationEntities;
using Benefit.Domain.Models;
using Benefit.Domain.Models.ModelExtensions;
using Benefit.Domain.Models.XmlModels;

namespace Benefit.Services.Domain
{
    public class CategoriesService
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private SellerService SellerService = new SellerService();
        private ProductsService ProductsService = new ProductsService();

        public Category GetByUrlWithChildren(string urlName)
        {
            var category =
                db.Categories.Include(entry => entry.ChildCategories).FirstOrDefault(entry => entry.UrlName == urlName);
            return category;
        }

        public Category GetCategoryByFullName(string categoryFullPass)
        {
            var parts = categoryFullPass.Split('/');
            var catName = parts.Last();
            var categories = db.Categories
                .AsNoTracking()
                .Include(entry=>entry.ParentCategory)
                .Include(entry=>entry.ProductParameters.Select(pp=>pp.ProductParameterValues))
                .Where(entry => entry.Name == catName).ToList();
            if (!categories.Any()) return null;
            if (categories.Count == 1) return categories.First();
            var parentName = parts[parts.Length - 2];
            var category = categories.First(entry => entry.ParentCategory.Name == parentName);
            return category;
        }

        public List<Category> GetBaseCategories()
        {
            return db.Categories.Where(entry => entry.ParentCategoryId == null).OrderBy(entry => entry.Order).ToList();
        }
        public Dictionary<Category,List<Category>> GetBreadcrumbs(string categoryId = null, string urlName = null)
        {
            var cacheKey = string.Format("{0}{1}{2}", CacheConstants.BreadCrumbsKey, categoryId, urlName);
            if (HttpRuntime.Cache[cacheKey] != null)
            {
                return HttpRuntime.Cache[CacheConstants.BreadCrumbsKey] as Dictionary<Category, List<Category>>;
            }
            var result = new Dictionary<Category, List<Category>>();
            var category = db.Categories.Include(entry => entry.ParentCategory).FirstOrDefault(entry => entry.Id == categoryId || entry.UrlName == urlName);
            if (category != null)
            {
                while (category.ParentCategory != null)
                {
                    var nextCats = db.Categories
                        .Where(entry =>
                            entry.ParentCategoryId == category.ParentCategory.Id && entry.IsActive && entry.Id != category.Id).ToList();
                    result.Add(category, nextCats);
                    category = category.ParentCategory;
                }
            }
            if (category != null)
            {
                result.Add(category, new List<Category>());
            }
            result = result.Reverse().ToDictionary(x => x.Key, x => x.Value);
            HttpRuntime.Cache.Add(CacheConstants.BreadCrumbsKey, result, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(3), CacheItemPriority.Default, null);
            return result;
        }

        public CategoriesViewModel GetCategoriesCatalog(string categoryUrl)
        {
            //fetch childs so if no nested categories - fetch products
            var parent = db.Categories.Include(entry=>entry.ChildCategories).FirstOrDefault(entry => entry.UrlName == categoryUrl);
            if (!string.IsNullOrEmpty(categoryUrl) && parent == null)
            {
                return null;
            }
            
            if (parent == null)
            {
                parent = new Category()
                {
                    Name = "Каталог"
                };
            }

            var categories =
                db.Categories
                    .Include(entry=>entry.ChildCategories)
                    .Include(entry=>entry.Products)
                    .Where(entry => entry.ParentCategoryId == parent.Id && entry.IsActive && !entry.IsSellerCategory && (entry.ChildCategories.Any() || entry.Products.Any()))
                    .OrderBy(entry => entry.Order)
                    .ToList();
            if (categories.Count == 1)
            {
                categories = categories.SelectMany(entry => entry.ChildCategories).ToList();
            }
            var catsModel = new CategoriesViewModel()
            {
                Category = parent,
                Items = categories.OrderBy(entry => entry.ChildCategories.Any()).ThenByDescending(entry => entry.Id == parent.Id).ToList(),
                Breadcrumbs = new BreadCrumbsViewModel()
                {
                    Categories = GetBreadcrumbs(parent.Id)
                }
            };
            return catsModel;
        }

        public CategoriesViewModel GetSellerCategoriesCatalog(Category parent, string sellerUrl)
        {
            var seller = db.Sellers.FirstOrDefault(entry => entry.UrlName.ToLower() == sellerUrl.ToLower());
            var sellerCategories = SellerService.GetAllSellerCategories(sellerUrl).ToList();

            List<Category> categories;
            if (parent == null || parent.ParentCategoryId == null)
            {
                categories =
                    sellerCategories.Where(entry => entry.ParentCategoryId == null)
                        .OrderBy(entry => entry.Order)
                        .ToList();
                if (categories.Count == 1)
                {
                    categories = categories.SelectMany(entry => entry.ChildCategories).ToList();
                }
                foreach (var category in categories)
                {
                    category.ChildCategories = category.ChildCategories.OrderBy(entry => entry.Order).ToList();
                }
            }
            else
            {
                categories = sellerCategories
                            .Where(
                                entry =>
                                    entry.ParentCategoryId == parent.Id)
                            .OrderBy(entry => entry.Order)
                            .ToList();
            }

            var parentId = parent == null ? null : parent.Id;
            var catsModel = new CategoriesViewModel()
            {
                Category = parent,
                Items = categories.OrderBy(entry => entry.ChildCategories.Any()).ThenByDescending(entry => entry.Id == parentId).ToList(),
                Seller = seller,
                Breadcrumbs = new BreadCrumbsViewModel()
                {
                    Categories = GetBreadcrumbs(parent == null ? null : parent.Id),
                    Seller = seller
                }
            };
            return catsModel;
        }

        //todo: remove comment think about product list
        /* public List<Product> GetCategoryProductsOnly(string categoryId, string sellerId, int skip, int take = ListConstants.DefaultTakePerPage)
         {
             var products = db.Products.Include(entry => entry.Images).Include(entry=>entry.Currency);
             if (!string.IsNullOrEmpty(categoryId))
             {
                 products = products.Where(entry => entry.CategoryId == categoryId);
             }
             if (!string.IsNullOrEmpty(sellerId))
             {
                 products = products.Where(entry => entry.SellerId == sellerId);
             }
             products = products.OrderBy(entry => entry.Category.Order).ThenByDescending(entry => entry.Images.Any());
             var result = products.Skip(skip).Take(take + 1).ToList();
             result.ForEach(entry => entry.Price = (double)(entry.Price * entry.Currency.Rate));
             return result;
         }*/

        /*public ProductsViewModel GetCategoryProducts(string urlName, string options, int skip = 0, int take = ListConstants.DefaultTakePerPage)
        {
            var category =
                db.Categories.Include(entry => entry.Products)
                    .Include(entry => entry.Products.Select(pr => pr.Images))
                    .Include(entry => entry.Products.Select(pr => pr.Currency))
                    .Include(entry => entry.ProductParameters.Select(pr=>pr.ProductParameterValues))
                    .FirstOrDefault(entry => entry.UrlName == urlName);
            if (category.Products.Count == 0)
            {
                return null;
            }
            var products = SellerService.GetSellerCatalogProducts(null, category.Id, options, skip, take);
            if (!products.Any()) return null;
            var result = new ProductsViewModel()
            {
                Items = products,
                Category = category,
                Breadcrumbs = new BreadCrumbsViewModel() { Categories = GetBreadcrumbs(urlName: urlName) }
            };
            return result;
        }*/

        public SellersViewModel GetCategorySellers(string urlName)
        {
            //which sellers to display
            var sellerIds = new List<string>();
            //get selected category with child categories and sellers
            var category =
                db.Categories.Include(entry => entry.ChildCategories)
                    .Include(entry => entry.SellerCategories)
                    .Include(entry => entry.MappedCategories)
                    .FirstOrDefault(entry => entry.UrlName == urlName);
            if (category == null) return null;
            var sellersDto = new SellersViewModel()
            {
                Category = category
            };
            var allChildCategories = category.GetAllChildrenRecursively().ToList();
            //add all sellers categories except default
            sellerIds.AddRange(category.SellerCategories.Select(entry => entry.SellerId));
            sellerIds.AddRange(category.MappedCategories.Select(entry => entry.SellerId));
            sellerIds.AddRange(allChildCategories.SelectMany(entry => entry.SellerCategories).Select(entry => entry.SellerId));
            sellerIds.AddRange(allChildCategories.SelectMany(entry => entry.MappedCategories).Select(entry => entry.SellerId));

            //filter by region and shippings
            var regionId = RegionService.GetRegionId();

            var items =
                db.Sellers
                    .Include(entry => entry.Images)
                    .Include(entry => entry.Addresses)
                    .Include(entry => entry.ShippingMethods.Select(sm => sm.Region))
                    .Include(entry => entry.SellerCategories.Select(sc => sc.Category.ParentCategory))
                    .Where(
                        entry =>
                            sellerIds.Contains(entry.Id) &&
                            entry.IsActive);

            if (regionId != RegionConstants.AllUkraineRegionId)
            {
                sellersDto.CurrentRegionItems =
                    items.Where(entry => entry.Addresses.Select(addr => addr.RegionId).Contains(regionId)).ToList();
            }
            var currentRegionItemsIds = sellersDto.CurrentRegionItems.Select(sc => sc.Id).ToList();
            sellersDto.Items = items.Where(entry => !currentRegionItemsIds.Contains(entry.Id)).
                OrderByDescending(entry => entry.Status).ThenByDescending(entry => entry.UserDiscount).ToList();

            sellersDto.Items.ForEach(entry =>
            {
                var tempAddresses = new List<Address>(entry.Addresses.ToList());
                entry.Addresses = new Collection<Address>();
                foreach (var address in tempAddresses.Where(addr => addr.RegionId == regionId))
                {
                    entry.Addresses.Add(address);
                }
                foreach (var address in tempAddresses.Where(addr => addr.RegionId != regionId))
                {
                    entry.Addresses.Add(address);
                }
                entry.ShippingMethods =
                    entry.ShippingMethods.Where(sh => sh.RegionId == entry.ShippingMethods.Min(shm => shm.RegionId))
                        .ToList();
            });
            sellersDto.Breadcrumbs = new BreadCrumbsViewModel { Categories = GetBreadcrumbs(category.Id) };
            return sellersDto;
        }

        public void Delete(string id)
        {
            var category = db.Categories.Include(entry => entry.SellerCategories).FirstOrDefault(entry => entry.Id == id);
            if (category == null) return;

            db.SellerCategories.RemoveRange(category.SellerCategories);

            var productParameters = category.ProductParameters;
            var productParameterValues =
                db.ProductParameterValues.Where(
                    entry => productParameters.Select(pr => pr.Id).Contains(entry.ProductParameterId));
            var productParameterProducts = db.ProductParameterProducts.Where(
                    entry => productParameters.Select(pr => pr.Id).Contains(entry.ProductParameterId));

            db.ProductParameterProducts.RemoveRange(productParameterProducts);
            db.ProductParameterValues.RemoveRange(productParameterValues);
            db.ProductParameters.RemoveRange(productParameters);
            db.Localizations.RemoveRange(db.Localizations.Where(entry => entry.ResourceId == category.Id));
            if (category.ImageUrl != null)
            {
                var image = new FileInfo(Path.Combine(HttpContext.Current.Server.MapPath("~/Images/"), category.ImageUrl));
                if (image.Exists)
                    image.Delete();
            }
            var products = db.Products.AsNoTracking().Where(entry => entry.CategoryId == id).ToList();
            foreach (var product in products)
            {
                ProductsService.Delete(product.Id);
            }

            var childCats = db.Categories.AsNoTracking().Where(entry => entry.ParentCategoryId == id).ToList();
            foreach (var childCategory in childCats)
            {
                Delete(childCategory.Id);
            }
            db.Categories.Remove(category);
            db.SaveChanges();
        }
    }
}
