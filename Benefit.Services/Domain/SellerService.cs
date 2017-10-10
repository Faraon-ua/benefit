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
using Benefit.DataTransfer.ViewModels.Base;
using Benefit.DataTransfer.ViewModels.NavigationEntities;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;
using Benefit.Domain.Models.ModelExtensions;

namespace Benefit.Services.Domain
{
    public class SellerService
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public string AddNotificationChannel(string sellerId, string channelAddress, NotificationChannelType channelType)
        {
            var seller = db.Sellers.Find(sellerId);
            if (seller == null) return null;
            var existingNotification =
                db.NotificationChannels.FirstOrDefault(
                    entry => entry.SellerId == sellerId && entry.Address == channelAddress);
            if (existingNotification != null) return seller.Name;
            var notificationCHannel = new NotificationChannel()
            {
                Id = Guid.NewGuid().ToString(),
                SellerId = sellerId,
                ChannelType = channelType,
                Address = channelAddress
            };
            db.NotificationChannels.Add(notificationCHannel);
            db.SaveChanges();
            return seller.Name;
        }
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
                Items = sellers.OrderByDescending(entry => entry.Status).ThenByDescending(entry => entry.Addresses.Any(addr => addr.RegionId == regionId)).ThenByDescending(entry => entry.UserDiscount).ToList()
            };
        }

        public Seller GetSellerWithShippingMethods(string urlName)
        {
            return db.Sellers.Include(entry => entry.ShippingMethods.Select(sh => sh.Region)).FirstOrDefault(entry => entry.UrlName == urlName);
        }
        public SellerDetailsViewModel GetSellerDetails(string urlName, string categoryUrlName, string currentUserId)
        {
            var sellerVM = new SellerDetailsViewModel();
            var seller = db.Sellers
                .Include(entry => entry.SellerCategories)
                .Include(entry => entry.Schedules)
                .Include(entry => entry.Addresses)
                .Include(entry => entry.ShippingMethods)
                .Include(entry => entry.Reviews.Select(review => review.ChildReviews))
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
                sellerVM.CanReview = db.Orders.Any(entry => entry.Status == OrderStatus.Finished && entry.UserId == currentUserId && entry.SellerId == seller.Id);
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
                    db.Sellers
                    .Include(entry => entry.SellerCategories.Select(sc => sc.Category))
                    .Include(entry => entry.MappedCategories.Select(mc => mc.MappedParentCategory))
                        .FirstOrDefault(entry => entry.UrlName == sellerUrl);
                var all = new List<Category>();
                var sellerCats = seller.SellerCategories.Select(entry => entry.Category);
                var sellerMappedCats = seller.MappedCategories.Where(entry => entry.MappedParentCategory != null).Select(entry => entry.MappedParentCategory).ToList();
                all.AddRange(sellerCats);
                all.AddRange(sellerMappedCats);
                foreach (var sellerCat in sellerCats)
                {
                    var parent = sellerCat.ParentCategory;
                    while (parent != null)
                    {
                        all.Add(parent);
                        parent = parent.ParentCategory;
                    }
                }
                foreach (var sellerCat in sellerMappedCats)
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

        public ProductsWithParametersList GetSellerCatalogProducts(string sellerId, string categoryId, string options, int skip = 0, int take = ListConstants.DefaultTakePerPage, bool fetchParameters = true)
        {
            var result = new ProductsWithParametersList();

            var items = db.Products.Include(entry => entry.Category.ParentCategory.ParentCategory)
                .Include(entry => entry.Seller)
                .Include(entry => entry.Seller.ShippingMethods.Select(sm => sm.Region))
                .Include(entry => entry.Seller.Addresses)
                .Include(entry => entry.ProductParameterProducts.Select(pr => pr.ProductParameter))
                .Where(entry => entry.IsActive && entry.Seller.IsActive && entry.Seller.HasEcommerce);

            if (!string.IsNullOrEmpty(categoryId))
            {
                var category = db.Categories.Include(entry => entry.MappedCategories).FirstOrDefault(entry => entry.Id == categoryId);
                var allIds = new List<string>();
                //add mapped categoryIds
                allIds.AddRange(category.MappedCategories.Select(entry => entry.Id));

                if (sellerId != null)
                {
                    var allDescendants = category.GetAllChildrenRecursively().ToList();
                    var allDescandantIds = allDescendants.Select(entry => entry.Id).ToList();
                    allIds.AddRange(allDescandantIds);
                    allIds.AddRange(allDescendants.SelectMany(entry => entry.MappedCategories).Select(entry => entry.Id));
                }
                allIds.Add(categoryId);
                items = items.Where(entry => allIds.Contains(entry.CategoryId));
            }
            if (!string.IsNullOrEmpty(sellerId))
            {
                items = items.Where(entry => entry.SellerId == sellerId);
            }
            //todo: instead all products show recomendations
            var sort = ProductSortOption.Order;
            if (options != null)
            {
                var optionSegments = options.Split(';');
                foreach (var optionSegment in optionSegments)
                {
                    if (optionSegment == string.Empty) continue;
                    var optionKeyValue = optionSegment.Split('=');
                    var optionKey = optionKeyValue.First();
                    var optionValues = optionKeyValue.Last().Split(',');
                    if (optionKey != "sort")
                    {
                        items =
                            items.Where(
                                entry =>
                                    entry.ProductParameterProducts.Any(pr => pr.ProductParameter.UrlName == optionKey) &&
                                    optionValues.Any(
                                        optValue =>
                                            entry.ProductParameterProducts.Select(pr => pr.StartValue)
                                                .Contains(optValue)));
                    }
                    else
                    {
                        if (optionKey == "sort")
                        {
                            sort = (ProductSortOption)Enum.Parse(typeof(ProductSortOption), optionValues.First());
                        }
                    }
                }
            }
            if (fetchParameters)
            {
                result.ProductParameters =
                    items.SelectMany(entry => entry.ProductParameterProducts.Select(pr => pr.StartValue)).ToList();
            }
            switch (sort)
            {
                case ProductSortOption.Order:
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

            result.Products = items.Skip(skip).Take(take + 1).ToList();
            result.Products.ForEach(entry => entry.Price = (double)(entry.Price * entry.Currency.Rate));

            return result;
        }

        public NavigationEntitiesViewModel<Product> GetSellerCatalog(string sellerUrl, string categoryUrl, string options)
        {
            var categoriesService = new CategoriesService();
            var result = new ProductsViewModel();
            var seller = db.Sellers.Include(entry => entry.SellerCategories.Select(sc => sc.Category)).FirstOrDefault(entry => entry.UrlName == sellerUrl);
            //            if (seller == null) return null;
            result.Seller = seller;
            var category = db.Categories.Include(entry => entry.ProductParameters.Select(pr => pr.ProductParameterValues)).FirstOrDefault(entry => entry.UrlName == categoryUrl);
            result.Category = category;
            var categoryId = category == null ? null : category.Id;
            var catalog = GetSellerCatalogProducts(seller == null ? null : seller.Id, categoryId, options);
            result.Items = catalog.Products.ToList();

            //product parameters
            if (category != null)
            {
                var productParameters = category.ProductParameters.Where(entry => entry.DisplayInFilters).ToList();
                if (seller != null)
                {
                    var mappedCategories = db.Categories.Include(
                        entry => entry.MappedCategories.Select(mc => mc.ProductParameters))
                        .Where(
                            entry =>
                                entry.SellerId == seller.Id && entry.MappedParentCategoryId == category.Id &&
                                entry.IsActive).ToList();
                    var mappedCategoriesParameters = mappedCategories
                        .SelectMany(entry => entry.ProductParameters)
                        .Where(entry => entry.DisplayInFilters).ToList();
                    productParameters = productParameters.Union(mappedCategoriesParameters).ToList();
                }
                foreach (var productParameter in productParameters)
                {
                    productParameter.ProductParameterValues =
                        productParameter.ProductParameterValues.Where(
                            entry => catalog.ProductParameters.Contains(entry.ParameterValueUrl)).ToList();
                }
                var productParameterNames = productParameters.Select(entry => entry.UrlName).Distinct().ToList();
                foreach (var productParameterName in productParameterNames)
                {
                    var parameters = productParameters.Where(entry => entry.UrlName == productParameterName).ToList();
                    var parameter = parameters.First();
                    parameter.ProductParameterValues =
                        parameters.SelectMany(entry => entry.ProductParameterValues).Distinct(new ProductParameterValueComparer()).ToList();
                    result.ProductParameters.Add(parameter);
                }
            }
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
