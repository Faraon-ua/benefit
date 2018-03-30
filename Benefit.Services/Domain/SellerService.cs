using Benefit.Common.Constants;
using Benefit.Common.Helpers;
using Benefit.DataTransfer.ViewModels;
using Benefit.DataTransfer.ViewModels.Base;
using Benefit.DataTransfer.ViewModels.NavigationEntities;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;
using Benefit.Domain.Models.ModelExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;

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

        public SellersViewModel GetSellersCatalog(string options, int page = 0)
        {
            var sellers = db.Sellers
                .Include(entry => entry.SellerCategories.Select(sc => sc.Category.ParentCategory))
                .Include(entry => entry.Addresses.Select(addr => addr.Region))
                .Where(entry=>entry.IsActive);

            var sort = SellerSortOption.Rating;

            if (options != null)
            {
                var optionSegments = options.Split(';');
                foreach (var optionSegment in optionSegments)
                {
                    if (optionSegment == string.Empty) continue;
                    var optionKeyValue = optionSegment.Split('=');
                    var optionKey = optionKeyValue.First();
                    var optionValues = optionKeyValue.Last().Split(',');
                    switch (optionKey)
                    {
                        case "sort":
                            sort = (SellerSortOption)Enum.Parse(typeof(SellerSortOption), optionValues.First());
                            break;
                        case "filter":
                            if (optionValues.Contains("mycity"))
                            {
                                var regionId = RegionService.GetRegionId();
                                sellers = sellers.Where(entry =>
                                    entry.Addresses.Select(addr => addr.RegionId).Contains(regionId));
                            }
                            if (optionValues.Contains("paymentcard"))
                            {
                                sellers = sellers.Where(entry => entry.IsAcquiringActive);
                            }
                            if (optionValues.Contains("paymentbonuses"))
                            {
                                sellers = sellers.Where(entry => entry.IsBonusesPaymentActive || entry.TerminalBonusesPaymentActive);
                            }
                            if (optionValues.Contains("freeshipping"))
                            {
                                sellers = sellers.Where(entry => entry.ShippingMethods.Any(sh => sh.FreeStartsFrom.HasValue));
                            }
                            if (optionValues.Contains("benefitcard"))
                            {
                                sellers = sellers.Where(entry => entry.IsBenefitCardActive);
                            }
                            if (optionValues.Contains("benefitonline"))
                            {
                                sellers = sellers.Where(entry => entry.HasEcommerce);
                            }
                            break;
                        case "category":
                            var categories = db.Categories.Where(entry => optionValues.Contains(entry.UrlName));
                            var catIds = categories.Select(entry => entry.Id).Distinct().ToList();
                            sellers = sellers.Where(entry =>
                                entry.SellerCategories.Select(sc => sc.Category.Id).Intersect(catIds).Any()
                                ||
                                entry.SellerCategories.Select(sc => sc.Category.ParentCategory.Id).Intersect(catIds)
                                    .Any());
                            break;
                    }
                }
            }
            switch (sort)
            {
                case SellerSortOption.Rating:
                    sellers = sellers.OrderByDescending(entry => entry.AvarageRating).ThenByDescending(entry => entry.Reviews.Count);
                    break;
                case SellerSortOption.NameAsc:
                    sellers = sellers.OrderBy(entry => entry.Name);
                    break;
                case SellerSortOption.NameDesc:
                    sellers = sellers.OrderByDescending(entry => entry.Name);
                    break;
                case SellerSortOption.BonusAsc:
                    sellers = sellers.OrderBy(entry => entry.UserDiscount);
                    break;
                case SellerSortOption.BonusDesc:
                    sellers = sellers.OrderByDescending(entry => entry.UserDiscount);
                    break;
            }
            return new SellersViewModel()
            {
                Items = sellers.Skip(ListConstants.DefaultTakePerPage * page).Take(ListConstants.DefaultTakePerPage + 1).ToList(),
                Category = new Category()
                {
                    UrlName = "postachalnuky",
                    Name = "Каталог постачальників",
                    ChildCategories = db.Categories.Where(entry => entry.ParentCategoryId == null && entry.IsActive && !entry.IsSellerCategory).ToList(),
                    ChildAsFilters = true
                }
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
                .Include(entry => entry.Reviews)
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
            var sort = ProductSortOption.Rating;
            if (options != null)
            {
                var optionSegments = options.Split(';');
                foreach (var optionSegment in optionSegments)
                {
                    if (optionSegment == string.Empty) continue;
                    var optionKeyValue = optionSegment.Split('=');
                    var optionKey = optionKeyValue.First();
                    var optionValues = optionKeyValue.Last().Split(',');
                    switch (optionKey)
                    {
                        case "sort":
                            sort = (ProductSortOption)Enum.Parse(typeof(ProductSortOption), optionValues.First());
                            break;
                        case "seller":
                            var sellerIds =
                                db.Sellers.Where(entry => optionValues.Contains(entry.UrlName))
                                    .Select(entry => entry.Id);
                            items = items.Where(entry => sellerIds.Contains(entry.SellerId));
                            break;
                        case "vendor":
                            items = items.Where(entry => optionValues.Contains(entry.Vendor));
                            break;
                        case "country":
                            items = items.Where(entry => optionValues.Contains(entry.OriginCountry));
                            break;
                        default:
                            items =
                            items.Where(
                                entry =>
                                    entry.ProductParameterProducts.Any(pr => pr.ProductParameter.UrlName == optionKey) &&
                                    optionValues.Any(
                                        optValue =>
                                            entry.ProductParameterProducts.Select(pr => pr.StartValue)
                                                .Contains(optValue)));
                            break;
                    }
                }
            }
            switch (sort)
            {
                case ProductSortOption.Rating:
                    items = items.OrderByDescending(entry => entry.AvarageRating).ThenBy(entry => entry.Name);
                    break;
                case ProductSortOption.Order:
                    items = items.OrderByDescending(entry => entry.Images.Any()).ThenBy(entry => entry.Name);
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

            //fetch product parameters
            if (categoryId != null && fetchParameters)
            {
                var productParameters = db.ProductParameters
                    .Include(entry => entry.ProductParameterProducts)
                    .Where(entry => entry.CategoryId == categoryId && entry.DisplayInFilters).ToList();
                if (sellerId != null)
                {
                    var mappedCategories = db.Categories
                        .Include(entry => entry.MappedCategories.Select(mc => mc.ProductParameters.Select(pp => pp.ProductParameterProducts)))
                        .Where(
                            entry =>
                                entry.SellerId == sellerId && entry.MappedParentCategoryId == categoryId &&
                                entry.IsActive).ToList();
                    var mappedCategoriesParameters = mappedCategories
                        .SelectMany(entry => entry.ProductParameters)
                        .Where(entry => entry.DisplayInFilters).ToList();
                    productParameters = productParameters.Union(mappedCategoriesParameters).ToList();
                }
                var productParametersInItems =
                    items.SelectMany(entry => entry.ProductParameterProducts.Select(pr => pr.StartValue))
                        .Distinct()
                        .ToList();
                var productParameterNames = productParameters.Select(entry => entry.UrlName).Distinct().ToList();
                foreach (var productParameterName in productParameterNames)
                {
                    var parameters = productParameters.Where(entry => entry.UrlName == productParameterName).ToList();
                    var parameter = parameters.First();
                    parameter.ProductParameterValues =
                        parameters.SelectMany(entry => entry.ProductParameterValues)
                            .Distinct(new ProductParameterValueComparer())
                            .ToList();

                    if (options != null && !options.Contains(parameter.UrlName))
                    {
                        parameter.ProductParameterValues =
                            parameter.ProductParameterValues.Where(
                                entry => productParametersInItems.Contains(entry.ParameterValueUrl)).ToList();
                    }

                    result.ProductParameters.Add(parameter);
                }

                productParameters.InsertRange(0, FetchGeneralProductParameters(items, sellerId == null));
                result.ProductParameters = productParameters;
            }

            result.Products = items.Skip(skip).Take(take + 1).ToList();
            result.Products.ForEach(entry => entry.Price = (double)(entry.Price * entry.Currency.Rate));

            return result;
        }

        public List<ProductParameter> FetchGeneralProductParameters(IQueryable<Product> items, bool fetchSellers)
        {
            var productParametersList = new List<ProductParameter>();
            if (fetchSellers)
            {
                var sellersParams =
                    items.Select(entry => entry.Seller).Distinct().ToList().Select(entry => new ProductParameterValue()
                    {
                        ParameterValue = entry.Name,
                        ParameterValueUrl = entry.UrlName
                    }).OrderBy(entry => entry.ParameterValue).ToList();
                productParametersList.Add(new ProductParameter()
                {
                    Name = "Постачальник",
                    UrlName = "seller",
                    Type = typeof(string).ToString(),
                    ProductParameterValues = sellersParams
                });
            }

            var vendorParams =
                   items.Select(entry => entry.Vendor).Distinct().ToList().Where(entry => !string.IsNullOrWhiteSpace(entry)).Select(entry => new ProductParameterValue()
                   {
                       ParameterValue = entry,
                       ParameterValueUrl = entry
                   }).OrderBy(entry => entry.ParameterValue).ToList();
            if (vendorParams.Count > 1)
            {
                productParametersList.Add(new ProductParameter()
                {
                    Name = "Виробник",
                    UrlName = "vendor",
                    Type = typeof(string).ToString(),
                    ProductParameterValues = vendorParams
                });
            }

            var originCountryParams =
                 items.Select(entry => entry.OriginCountry).Distinct().ToList().Where(entry => !string.IsNullOrWhiteSpace(entry)).Select(entry => new ProductParameterValue()
                 {
                     ParameterValue = entry,
                     ParameterValueUrl = entry
                 }).OrderBy(entry => entry.ParameterValue).ToList();
            if (originCountryParams.Count > 1)
            {
                productParametersList.Add(new ProductParameter()
                {
                    Name = "Країна виробник",
                    UrlName = "country",
                    Type = typeof(string).ToString(),
                    ProductParameterValues = originCountryParams
                });
            }
            return productParametersList;
        }

        public NavigationEntitiesViewModel<Product> GetSellerProductsCatalog(string sellerUrl, string categoryUrl, string options)
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
            result.ProductParameters = catalog.ProductParameters;

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
