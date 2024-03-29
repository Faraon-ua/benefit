﻿using System;
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
using Benefit.DataTransfer.ViewModels.NavigationEntities;
using Benefit.Domain.Models;
using Benefit.Domain.Models.ModelExtensions;

namespace Benefit.Services.Domain
{
    public class CategoriesService
    {
        private ProductsService ProductsService = new ProductsService();

        public Category GetByUrlWithChildren(string urlName)
        {
            using (var db = new ApplicationDbContext())
            {
                var category =
                db.Categories.Include(entry => entry.ChildCategories).FirstOrDefault(entry => entry.UrlName == urlName);
                return category;
            }
        }

        public Category GetCategoryByFullName(string categoryFullPass)
        {
            using (var db = new ApplicationDbContext())
            {
                var parts = categoryFullPass.Split('/');
                var catName = parts.Last();
                var categories = db.Categories
                    .AsNoTracking()
                    .Include(entry => entry.ParentCategory)
                    .Include(entry => entry.ProductParameters.Select(pp => pp.ProductParameterValues))
                    .Where(entry => entry.Name == catName && !entry.IsSellerCategory).ToList();
                if (!categories.Any())
                    return null;
                if (categories.Count == 1) return categories.First();
                var parentName = parts[parts.Length - 2];
                var category = categories.First(entry => entry.ParentCategory.Name == parentName);
                return category;
            }
        }

        public List<Category> GetBaseCategories()
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Categories.Where(entry => entry.ParentCategoryId == null).OrderBy(entry => entry.Order).ToList();
            }
        }
        public Dictionary<CategoryVM, List<CategoryVM>> GetBreadcrumbs(IEnumerable<CategoryVM> cachedCats, string categoryId = null, string urlName = null)
        {
            var result = new Dictionary<CategoryVM, List<CategoryVM>>();
            var category = cachedCats.FindByUrlIdRecursively(urlName, categoryId);
            if (category != null)
            {
                while (category.ParentCategory != null)
                {
                    var nextCats = category.ParentCategory.ChildCategories.Where(entry => entry.Id != category.Id).ToList();
                    result.Add(category, nextCats);
                    category = category.ParentCategory;
                }
            }
            if (category != null)
            {
                result.Add(category, new List<CategoryVM>());
            }
            result = result.Reverse().ToDictionary(x => x.Key, x => x.Value);
            return result;
        }

        public CategoriesViewModel GetCategoriesCatalog(IEnumerable<CategoryVM> cachedCats, string categoryUrl, string sellerUrl = null)
        {
            using (var db = new ApplicationDbContext())
            {
                //fetch childs so if no nested categories - fetch products
                var parent = cachedCats.FindByUrlIdRecursively(categoryUrl, null);
                if (!string.IsNullOrEmpty(categoryUrl) && parent == null)
                {
                    return null;
                }

                if (parent == null)
                {
                    var mainPage = db.InfoPages.FirstOrDefault(entry => entry.UrlName == "golovna");
                    parent = new CategoryVM()
                    {
                        Name = "Каталог",
                        Title = "Каталог товарів та послуг" + (mainPage == null ? string.Empty : mainPage.Title),
                        ChildCategories = cachedCats.ToList()
                    };
                }

                //if (sellerUrl != null)
                //{
                //    var sellerService = new SellerService();
                //    var sellerCats = sellerService.GetAllSellerCategories(sellerUrl);
                //    categories = categories.Intersect(sellerCats, new CategoryComparer()).ToList();
                //}
                //if (categories.Count == 1)
                //{
                //    categories = categories.SelectMany(entry => entry.ChildCategories).ToList();
                //}
                if (parent.BannerImageUrl == null && parent.ParentCategory != null)
                    parent.BannerImageUrl = parent.ParentCategory.BannerImageUrl;
                var catsModel = new CategoriesViewModel()
                {
                    Category = parent,
                    Items = parent.ChildCategories.ToList(),
                    Breadcrumbs = new BreadCrumbsViewModel()
                    {
                        Categories = GetBreadcrumbs(cachedCats, parent.Id)
                    }
                };
                return catsModel;
            }
        }
        //public CategoriesViewModel GetSellerCategoriesCatalog(Category parent, string sellerUrl)
        //{
        //    var seller = db.Sellers.FirstOrDefault(entry => entry.UrlName.ToLower() == sellerUrl.ToLower());
        //    var sellerCategories = SellerService.GetAllSellerCategories(sellerUrl).ToList();

        //    List<Category> categories;
        //    if (parent == null || parent.ParentCategoryId == null)
        //    {
        //        categories =
        //            sellerCategories.Where(entry => entry.ParentCategoryId == null)
        //                .OrderBy(entry => entry.Order)
        //                .ToList();
        //        if (categories.Count == 1)
        //        {
        //            categories = categories.SelectMany(entry => entry.ChildCategories).ToList();
        //        }
        //        foreach (var category in categories)
        //        {
        //            category.ChildCategories = category.ChildCategories.OrderBy(entry => entry.Order).ToList();
        //        }
        //    }
        //    else
        //    {
        //        categories = sellerCategories
        //                    .Where(
        //                        entry =>
        //                            entry.ParentCategoryId == parent.Id)
        //                    .OrderBy(entry => entry.Order)
        //                    .ToList();
        //    }

        //    var parentId = parent == null ? null : parent.Id;
        //    var catsModel = new CategoriesViewModel()
        //    {
        //        Category = parent,
        //        Items = categories.OrderBy(entry => entry.ChildCategories.Any()).ThenByDescending(entry => entry.Id == parentId).ToList(),
        //        Seller = seller,
        //        Breadcrumbs = new BreadCrumbsViewModel()
        //        {
        //            Categories = GetBreadcrumbs(parent == null ? null : parent.Id),
        //            Seller = seller
        //        }
        //    };
        //    return catsModel;
        //}

        public SellersViewModel GetCategorySellers(IEnumerable<CategoryVM> cachedCats, string urlName)
        {
            using (var db = new ApplicationDbContext())
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
                    Category = category.MapToVM()
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
                sellersDto.Breadcrumbs = new BreadCrumbsViewModel { Categories = GetBreadcrumbs(cachedCats, category.Id) };
                return sellersDto;
            }
        }

        public void Delete(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var category = db.Categories
                    .Include(entry => entry.MappedCategories)
                    .Include(entry => entry.SellerCategories)
                    .Include(entry => entry.ProductParameters)
                    .Include(entry => entry.ProductOptions)
                    .Include(entry => entry.ExportCategories)
                    .FirstOrDefault(entry => entry.Id == id);
                Delete(category, db);
            }
        }
        public void Delete(Category category, ApplicationDbContext db)
        {
            if (category == null) return;
            var mappedCats = category.MappedCategories.ToList();
            foreach (var mappedCategory in mappedCats)
            {
                Delete(mappedCategory, db);
            }
            db.SellerCategories.RemoveRange(category.SellerCategories);

            var productParameters = category.ProductParameters.ToList();
            var productParameterIds = productParameters.Select(entry => entry.Id).ToList();
            var productParameterValues =
                db.ProductParameterValues.Where(
                    entry => productParameterIds.Contains(entry.ProductParameterId)).ToList();
            var productParameterProducts = db.ProductParameterProducts.Where(
                    entry => productParameterIds.Contains(entry.ProductParameterId)).ToList();

            db.ExportCategories.RemoveRange(category.ExportCategories);
            db.ProductParameterProducts.RemoveRange(productParameterProducts);
            db.ProductParameterValues.RemoveRange(productParameterValues);
            db.ProductParameters.RemoveRange(productParameters);
            db.ProductOptions.RemoveRange(category.ProductOptions.ToList());
            db.Localizations.RemoveRange(db.Localizations.Where(entry => entry.ResourceId == category.Id));
            if (category.ImageUrl != null)
            {
                var image = new FileInfo(Path.Combine(HttpContext.Current.Server.MapPath("~/Images/"), category.ImageUrl));
                if (image.Exists)
                    image.Delete();
            }
            db.SaveChanges();
            var products = db.Products
                        .Include(entry => entry.Images)
                        .Include(entry => entry.ProductOptions)
                        .Include(entry => entry.StatusStamps)
                        .Include(entry => entry.ProductParameterProducts).Where(entry => entry.CategoryId == category.Id).ToList();
            foreach (var product in products)
            {
                ProductsService.Delete(product, db);
            }
            var childCats = db.Categories
                .Include(entry => entry.MappedCategories)
                    .Include(entry => entry.SellerCategories)
                    .Include(entry => entry.ProductParameters)
                    .Include(entry => entry.ProductOptions)
                    .Include(entry => entry.ExportCategories)
                    .Where(entry => entry.ParentCategoryId == category.Id).ToList();
            foreach (var childCategory in childCats)
            {
                Delete(childCategory, db);
            }
            db.Categories.Attach(category);
            db.Categories.Remove(category);
            db.SaveChanges();
        }
    }
}
