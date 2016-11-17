using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using System.Data.Entity;
using Benefit.Domain.Models;

namespace Benefit.Services.Domain
{
    public class SellerService
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public SellerDetailsViewModel GetSellerDetails(string urlName, string categoryUrlName)
        {
            var sellerVM = new SellerDetailsViewModel();
            var seller = db.Sellers
                .Include(entry => entry.SellerCategories)
                .Include(entry => entry.Schedules)
                .Include(entry => entry.Addresses)
                .Include(entry => entry.ShippingMethods)
                .Include(entry => entry.ShippingMethods.Select(sm => sm.Region))
                .FirstOrDefault(entry => entry.UrlName == urlName);
            if (seller != null)
            {
                var regionId = RegionService.GetRegionId();
                var tempAddresses = new List<Address>(seller.Addresses.ToList());
                seller.Addresses = new Collection<Address>();
                foreach (var address in tempAddresses.Where(addr => addr.RegionId == regionId))
                {
                    seller.Addresses.Add(address);
                }
                foreach (var address in tempAddresses.Where(addr => addr.RegionId != regionId))
                {
                    seller.Addresses.Add(address);
                }

                sellerVM.Seller = seller;
                var categoriesService = new CategoriesService();
                sellerVM.Breadcrumbs = new BreadCrumbsViewModel
                {
                    Seller = seller
                };
                if (categoryUrlName == null)
                {
                    sellerVM.Breadcrumbs.Categories = categoriesService.GetBreadcrumbs(seller.SellerCategories.First().CategoryId);
                }
                else
                {
                    sellerVM.Breadcrumbs.Categories = categoriesService.GetBreadcrumbs(urlName: categoryUrlName);
                }
            }
            return sellerVM;
        }

        public List<Category> GetAllSellerCategories(string sellerUrl)
        {
            var cacheCats =
                HttpContext.Current.Cache[string.Format("{0}-{1}", CacheConstants.SellerCategoriessKey, sellerUrl)];
            if (cacheCats == null)
            {
                var seller =
                    db.Sellers.Include(entry => entry.SellerCategories.Select(sc => sc.Category))
                        .FirstOrDefault(entry => entry.UrlName == sellerUrl);
                var all = new List<Category>();
                var sellerCats = seller.SellerCategories.Select(entry => entry.Category);
                all.AddRange(sellerCats);
                foreach (var sellerCat in sellerCats)
                {
                    var parent = sellerCat.ParentCategory;
                    while (parent != null)
                    {
                        all.Add(parent);
                        parent = parent.ParentCategory;
                    }
                }
                cacheCats = all.Distinct(new CategoryComparer()).ToList();
                HttpContext.Current.Cache.Add(string.Format("{0}-{1}", CacheConstants.SellerCategoriessKey, seller.Id),
                    cacheCats, null, Cache.NoAbsoluteExpiration,
                    new TimeSpan(0, 0, CacheConstants.OutputCacheLength, 0),
                    CacheItemPriority.Default, null);
            }
            return cacheCats as List<Category>;
        }

        public ProductsViewModel GetSellerCatalog(string sellerUrl, string categoryUrl)
        {
            var result = new ProductsViewModel();
            var seller = db.Sellers.Include(entry => entry.SellerCategories.Select(sc => sc.Category)).FirstOrDefault(entry => entry.UrlName == sellerUrl);
            if (seller == null) return null;
            result.Seller = seller;
            var category = db.Categories.FirstOrDefault(entry => entry.UrlName == categoryUrl);
            result.Category = category;
            result.Items = new List<Product>();
            result.Breadcrumbs = new BreadCrumbsViewModel()
            {
                Seller = seller,
                Categories = new List<Category>() //{ seller.SellerCategories.FirstOrDefault(entry=>entry.IsDefault).Category }
            };
            return result;
        }

        public void ProcessCategories(List<SellerCategory> categories, string sellerId)
        {
            var toRemove = db.SellerCategories.Where(entry => entry.SellerId == sellerId).ToList();
            db.SellerCategories.RemoveRange(toRemove);
            if (!categories.Any())
            {
                db.SaveChanges();
                return;
            }
            categories.ForEach(entry =>
            {
                entry.SellerId = sellerId;
            });
            db.SellerCategories.AddRange(categories);
            db.SaveChanges();
        }

        public void ProcessCurrencies(List<Currency> currencies, string sellerId)
        {
            var toRemove = db.Currencies.Where(entry => entry.SellerId == sellerId).ToList();
            db.Currencies.RemoveRange(toRemove);
            if (!currencies.Any())
            {
                db.SaveChanges();
                return;
            }
            currencies.ForEach(entry =>
            {
                entry.Id = Guid.NewGuid().ToString();
                entry.SellerId = sellerId;
            });
            db.Currencies.AddRange(currencies);
            db.SaveChanges();
        }

        public void ProcessAddresses(List<Address> addresses, string sellerId)
        {
            var toRemove = db.Addresses.Where(entry => entry.SellerId == sellerId).ToList();
            db.Addresses.RemoveRange(toRemove);
            if (!addresses.Any())
            {
                db.SaveChanges();
                return;
            }
            addresses.ForEach(entry =>
            {
                entry.Id = Guid.NewGuid().ToString();
                entry.SellerId = sellerId;
            });
            db.Addresses.AddRange(addresses);
            db.SaveChanges();
        }
        public void ProcessShippingMethods(List<ShippingMethod> shippingMethods, string sellerId)
        {
            var toRemove = db.ShippingMethods.Where(entry => entry.SellerId == sellerId).ToList();
            db.ShippingMethods.RemoveRange(toRemove);
            if (!shippingMethods.Any())
            {
                db.SaveChanges();
                return;
            }
            shippingMethods.ForEach(entry =>
            {
                entry.Id = Guid.NewGuid().ToString();
                entry.SellerId = sellerId;
            });
            db.ShippingMethods.AddRange(shippingMethods);
            db.SaveChanges();
        }
    }
}
