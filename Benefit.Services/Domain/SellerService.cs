using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using Benefit.Common.Constants;
using Benefit.Common.Helpers;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using System.Data.Entity;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;
using Benefit.Domain.Models.ModelExtensions;

namespace Benefit.Services.Domain
{
    public class SellerService
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public byte[] GenerateIntelHexFile(string password)
        {
            const int bytesInLine = 0x10; //16 bytes

            var result = new StringBuilder();
            var hexPassword = HexHelper.AsciiToHexString(password);
            var linesNumber = 32;
            var allData = HexHelper.AddGarbage(linesNumber * bytesInLine, hexPassword, 16, 48, "FF");
            var linePrefix = ":";
            var startAddress = 0x0000;
            var fileStart = ":020000040000FA";
            var fileEnding = ":00000001FF";
            var dataType = "00";

            result.Append(fileStart);
            result.Append(Environment.NewLine);
            for (var i = 0; i < linesNumber; i++)
            {
                var line = new StringBuilder();
                var lineStart = string.Concat(bytesInLine.ToString("X"),
                    (startAddress + (bytesInLine * i)).ToString("X4"), dataType);
                line.Append(lineStart);

                line.Append(allData.Substring(i * bytesInLine * 2, bytesInLine * 2));

                var bytesSum = HexHelper.SumHexBytesInString(line.ToString());
                var checkSum = (byte)(0 - (bytesSum % 256));
                line.Append(checkSum.ToString("X2"));
                line.Insert(0, linePrefix);
                result.Append(line);
                result.Append(Environment.NewLine);
            }
            result.Append(fileEnding);
            result.Append(Environment.NewLine);
            return HexHelper.GetBytes(result.ToString());
        }

        public SellersViewModel GetAllSellers()
        {
            var regionId = RegionService.GetRegionId();

            var sellers =
                db.Sellers
                 .Include(entry => entry.Images)
                    .Include(entry => entry.Addresses)
                    .Include(entry => entry.ShippingMethods.Select(sm => sm.Region))
                    .Include(entry => entry.SellerCategories.Select(sc => sc.Category.ParentCategory))
                     .Where(entry => entry.IsActive);
            if (regionId != RegionConstants.AllUkraineRegionId)
            {
                sellers = sellers.Where(entry => entry.Addresses.Any(addr => addr.RegionId == regionId) ||
                                                 entry.ShippingMethods.Select(sm => sm.Region.Id)
                                                     .Contains(RegionConstants.AllUkraineRegionId) ||
                                                 entry.ShippingMethods.Select(sm => sm.Region.Id).Contains(regionId));
            }
            return new SellersViewModel()
            {
                Items = sellers.OrderByDescending(entry => entry.Addresses.Any(addr => addr.RegionId == regionId)).ThenByDescending(entry => entry.UserDiscount).ToList()
            };
        }

        public Seller GetSellerWithShippingMethods(string urlName)
        {
            return db.Sellers.Include(entry => entry.ShippingMethods.Select(sh => sh.Region)).FirstOrDefault(entry => entry.UrlName == urlName);
        }
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
                sellerVM.Breadcrumbs.IsInfoPage = true;
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

        public List<Product> GetSellerCatalogProducts(string sellerId, string categoryId, ProductSortOption sort, int skip = 0, int take = ListConstants.DefaultTakePerPage)
        {
            var regionId = RegionService.GetRegionId();
            var items = db.Products.Include(entry => entry.Category.ParentCategory.ParentCategory)
                .Include(entry => entry.Seller)
                .Include(entry => entry.Seller.ShippingMethods.Select(sm => sm.Region))
                .Include(entry => entry.Seller.Addresses)
                .Where(entry => entry.IsActive && entry.Seller.IsActive && entry.Seller.HasEcommerce);
            if (regionId != RegionConstants.AllUkraineRegionId)
            {
                items = items.Where(entry => entry.Seller.Addresses.Any(addr => addr.RegionId == regionId) ||
                                entry.Seller.ShippingMethods.Select(sm => sm.Region.Id).Contains(RegionConstants.AllUkraineRegionId) ||
                                entry.Seller.ShippingMethods.Select(sm => sm.Region.Id).Contains(regionId));
            }

            if (!string.IsNullOrEmpty(categoryId))
            {
                var category = db.Categories.Find(categoryId);
                var allDescendants = category.GetAllChildrenRecursively();
                var allIds = allDescendants.ToList().Select(entry => entry.Id).ToList();
                allIds.Add(categoryId);
                items = items.Where(entry => allIds.Contains(entry.CategoryId));
            }
            if (!string.IsNullOrEmpty(sellerId))
            {
                items = items.Where(entry => entry.SellerId == sellerId);
            }
            //todo: instead all products show recomendations
            switch (sort)
            {
                case ProductSortOption.Default:
                    items = items
                        .OrderBy(
                            entry =>
                                entry.Category.ParentCategory == null
                                    ? 1000
                                    : entry.Category.ParentCategory.ParentCategory == null
                                        ? 1000
                                        : entry.Category.ParentCategory.ParentCategory.Order)
                        .ThenBy(
                            entry => entry.Category.ParentCategory == null ? 1000 : entry.Category.ParentCategory.Order)
                        .ThenBy(entry => entry.Category.Order)
                        .ThenByDescending(entry => entry.Images.Any());
                    break;
                case ProductSortOption.NameAsc:
                    items = items.OrderBy(entry => entry.Name);
                    break;
                case ProductSortOption.NameDesc:
                    items = items.OrderByDescending(entry => entry.Name);
                    break;
                case ProductSortOption.PriceAsc:
                    items = items.OrderBy(entry => entry.Price);
                    break;
                case ProductSortOption.PriceDesc:
                    items = items.OrderByDescending(entry => entry.Price);
                    break;
            }

            var result = items.Skip(skip).Take(take + 1).ToList();
            result.ForEach(entry => entry.Price = (double)(entry.Price * entry.Currency.Rate));

            return result;
        }

        public ProductsViewModel GetSellerCatalog(string sellerUrl, string categoryUrl, ProductSortOption sort)
        {
            var categoriesService = new CategoriesService();
            var result = new ProductsViewModel();
            var seller = db.Sellers.Include(entry => entry.SellerCategories.Select(sc => sc.Category)).FirstOrDefault(entry => entry.UrlName == sellerUrl);
            if (seller == null) return null;
            result.Seller = seller;
            var category = db.Categories.FirstOrDefault(entry => entry.UrlName == categoryUrl);
            result.Category = category;
            var categoryId = category == null ? null : category.Id;
            result.Items = GetSellerCatalogProducts(seller.Id, categoryId, sort).ToList();
            //todo: add breadcrumbs
            result.Breadcrumbs = new BreadCrumbsViewModel()
            {
                Seller = seller,
                Categories = categoriesService.GetBreadcrumbs(category == null ? null : category.Id)
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
