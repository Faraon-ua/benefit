using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web;
using Benefit.Common.Constants;
using Benefit.Common.Extensions;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using System.Data.Entity;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;
using Benefit.Domain.Models.ModelExtensions;
using Benefit.Domain.Models.XmlModels;

namespace Benefit.Services.Domain
{
    public class CategoriesService
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private SellerService SellerService = new SellerService();

        public List<Category> GetBaseCategories()
        {
            return db.Categories.Where(entry => entry.ParentCategoryId == null).OrderBy(entry => entry.Order).ToList();
        } 
        public List<Category> GetBreadcrumbs(string categoryId = null, string urlName = null)
        {
            var cacheKey = string.Format("{0}{1}{2}", CacheConstants.BreadCrumbsKey, categoryId, urlName);
            if (HttpRuntime.Cache[cacheKey] != null)
            {
                return HttpRuntime.Cache[CacheConstants.BreadCrumbsKey] as List<Category>;
            }
            var resultList = new List<Category>();
            var category = db.Categories.Include(entry => entry.ParentCategory).FirstOrDefault(entry => entry.Id == categoryId || entry.UrlName == urlName);
            if (category != null)
            {
                resultList.Add(category);
                while (category.ParentCategory != null)
                {
                    resultList.Add(category.ParentCategory);
                    category = category.ParentCategory;
                }
            }
            resultList.Reverse();
            return resultList;
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

        public ProductsViewModel GetCategoryProducts(string urlName, int skip = 0, int take = ListConstants.DefaultTakePerPage)
        {
            var category = db.Categories.Include(entry => entry.Products).Include(entry => entry.Products.Select(pr => pr.Images)).Include(entry => entry.Products.Select(pr => pr.Currency)).FirstOrDefault(entry => entry.UrlName == urlName);
            if (category.Products.Count == 0)
            {
                return null;
            }
            var products = SellerService.GetSellerCatalogProducts(null, category.Id, ProductSortOption.Default, skip, take);
            if (!products.Any()) return null;
            var result = new ProductsViewModel()
            {
                Items = products,
                Category = category,
                Breadcrumbs = new BreadCrumbsViewModel() { Categories = GetBreadcrumbs(urlName: urlName) }
            };
            return result;
        }

        public SellersViewModel GetCategorySellers(string urlName)
        {
            //which sellers to display
            var sellerIds = new List<string>();
            //get selected category with child categories and sellers
            var category = db.Categories.Include(entry => entry.ChildCategories).Include(entry => entry.SellerCategories).FirstOrDefault(entry => entry.UrlName == urlName);
            if (category == null) return null;
            var sellersDto = new SellersViewModel()
            {
                Category = category
            };
            //add all sellers categories except default
            sellerIds.AddRange(category.SellerCategories.Select(entry => entry.SellerId));
            sellerIds.AddRange(category.GetAllChildrenRecursively().SelectMany(entry => entry.SellerCategories).Select(entry => entry.SellerId));

            //filter by region and shippings
            var regionId = RegionService.GetRegionId();
            sellersDto.Items =
                db.Sellers
                    .Include(entry => entry.Images)
                    .Include(entry => entry.Addresses)
                    .Include(entry => entry.ShippingMethods.Select(sm => sm.Region))
                    .Include(entry => entry.SellerCategories.Select(sc => sc.Category.ParentCategory))
                    .Where(
                        entry =>
                            sellerIds.Contains(entry.Id) &&
                            entry.IsActive &&
                            //todo: order by selected region
                            (entry.Addresses.Select(addr => addr.RegionId).Contains(regionId) ||
                             entry.ShippingMethods.Select(sm => sm.Region.Id)
                                 .Contains(RegionConstants.AllUkraineRegionId))
                    ).ToList();

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

        public void AddOrUpdateCategoriesFromXml(Seller seller, IEnumerable<XmlCategory> xmlCategories)
        {
            var defaultCategory = seller.SellerCategories.First(entry => entry.IsDefault).Category;
            var allDbCategories = defaultCategory.GetAllChildrenRecursively().ToList();

            var xmlToDbCategoriesMapping = new Dictionary<string, string>();
            foreach (var dbCategory in allDbCategories)
            {
                var xmlCategory = xmlCategories.FirstOrDefault(entry => entry.Name == dbCategory.Name);
                if (xmlCategory != null)
                {
                    xmlToDbCategoriesMapping.Add(xmlCategory.Id, dbCategory.Id);
                }
            }
        }

        private void SaveCategoryFromXmlCategory(XmlCategory xmlCategory, IEnumerable<XmlCategory> xmlCategories)
        {
            var existingCategory = db.Categories.Find(xmlCategory.Id);
            if (existingCategory != null)
            {
                existingCategory.Name = xmlCategory.Name;
                existingCategory.ParentCategoryId = xmlCategory.ParentId;
                db.Entry(existingCategory).State = EntityState.Modified;
                existingCategory.LastModified = DateTime.UtcNow;
                existingCategory.LastModifiedBy = "1CFileImport";
            }
            else
            {
                db.Categories.Add(new Category()
                {
                    Id = xmlCategory.Id,
                    Name = xmlCategory.Name,
                    UrlName = xmlCategory.Name.Translit(),
                    ParentCategoryId = xmlCategory.ParentId,
                    IsActive = true,
                    NavigationType = CategoryNavigationType.SellersAndProducts.ToString(),
                    LastModified = DateTime.UtcNow,
                    LastModifiedBy = "1CFileImport"
                });
            }

            var childCategories = xmlCategories.Where(entry => entry.ParentId == xmlCategory.Id).ToList();
            if (xmlCategories.Any())
            {
                childCategories.ForEach(entry => SaveCategoryFromXmlCategory(entry, xmlCategories));
            }
        }

        public void Delete(string id)
        {
            var category = db.Categories.Include(entry=>entry.SellerCategories).Include(entry=>entry.Products).Include(entry=>entry.ChildCategories).FirstOrDefault(entry => entry.Id == id);
            if (category == null) return;

            db.SellerCategories.RemoveRange(category.SellerCategories);
            
            db.ProductParameters.RemoveRange(category.ProductParameters);
            db.Localizations.RemoveRange(db.Localizations.Where(entry => entry.ResourceId == category.Id));
            if (category.ImageUrl != null)
            {
                var image = new FileInfo(Path.Combine(HttpContext.Current.Server.MapPath("~/Images/"), category.ImageUrl));
                if (image.Exists)
                    image.Delete();
            }

            foreach (var childCategory in category.ChildCategories)
            {
                Delete(childCategory.Id);
            }
            db.Categories.Remove(category);
            db.SaveChanges();
        }
    }
}
