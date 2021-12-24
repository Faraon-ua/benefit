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
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Benefit.Services.Domain
{
    public class SellerService
    {
        public string AddNotificationChannel(string sellerId, string channelAddress, NotificationChannelType channelType)
        {
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers.Find(sellerId);
                if (seller == null)
                {
                    return null;
                }

                var existingNotification =
                    db.NotificationChannels.FirstOrDefault(
                        entry => entry.SellerId == sellerId && entry.Address == channelAddress);
                if (existingNotification != null)
                {
                    return seller.Name;
                }

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
            using (var db = new ApplicationDbContext())
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
        }

        public SellersViewModel GetSellersCatalog(string options)
        {
            using (var db = new ApplicationDbContext())
            {
                var result = new SellersViewModel() { Page = 1 };
                var sellers = db.Sellers
                    .Include(entry => entry.SellerCategories.Select(sc => sc.Category.ParentCategory))
                    .Include(entry => entry.Addresses.Select(addr => addr.Region))
                    .Include(entry => entry.Schedules)
                    .Include(entry => entry.Images)
                    .Where(entry => entry.IsActive);

                var sort = SellerSortOption.Rating;

                if (options != null)
                {
                    var optionSegments = options.Split(';').OrderBy(entry => entry.Contains("page=")).ToList();
                    foreach (var optionSegment in optionSegments)
                    {
                        if (optionSegment == string.Empty)
                        {
                            continue;
                        }

                        var optionKeyValue = optionSegment.Split('=');
                        var optionKey = optionKeyValue.First();
                        var optionValues = optionKeyValue.Last().Split(',');
                        switch (optionKey)
                        {
                            case "sort":
                                sort = (SellerSortOption)Enum.Parse(typeof(SellerSortOption), optionValues.First());
                                break;
                            case "page":
                                switch (sort)
                                {
                                    case SellerSortOption.Rating:
                                        sellers = sellers.OrderByDescending(entry => entry.AvarageRating).ThenByDescending(entry => entry.Status).ThenByDescending(entry => entry.Reviews.Count);
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
                                result.Page = int.Parse(optionValues[0]);
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
                                //var categories = db.Categories.Where(entry => optionValues.Contains(entry.UrlName));
                                //var catIds = categories.Select(entry => entry.Id).Distinct().ToList();
                                //sellers = sellers.Where(entry =>
                                //    entry.SellerCategories.Select(sc => sc.Category.Id).Intersect(catIds).Any()
                                //    ||
                                //    entry.SellerCategories.Select(sc => sc.Category.ParentCategory.Id).Intersect(catIds)
                                //        .Any());
                                var catNames = optionValues.ToList().Select(entry => entry.Replace("_", " ")).ToList();
                                sellers = sellers.Where(entry => catNames.Contains(entry.CategoryName));
                                break;
                        }
                    }
                }
                switch (sort)
                {
                    case SellerSortOption.Rating:
                        sellers = sellers.OrderByDescending(entry => entry.AvarageRating).ThenByDescending(entry => entry.Status).ThenByDescending(entry => entry.Reviews.Count);
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
                result.PagesCount = (sellers.Count() - 1) / ListConstants.DefaultTakePerPage + 1;
                result.Items = sellers.Skip(ListConstants.DefaultTakePerPage * (result.Page - 1)).Take(ListConstants.DefaultTakePerPage + 1).ToList();
                result.Category = new CategoryVM()
                {
                    UrlName = "postachalnuky",
                    Name = "Каталог постачальників",
                    ChildAsFilters = true,
                    ChildCategories = db.Sellers.Where(entry => entry.CategoryName != null).Select(entry => entry.CategoryName).Distinct().Select(entry => new CategoryVM() { Name = entry }).ToList(),
                };
                return result;
            }
        }

        public Seller GetSellerWithShippingMethods(string urlName)
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Sellers.Include(entry => entry.ShippingMethods.Select(sh => sh.Region)).FirstOrDefault(entry => entry.UrlName == urlName);
            }
        }

        public Seller GetSeller(string urlName)
        {
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers
                .Include(entry => entry.InfoPages)
                .Include(entry => entry.Banners)
                .Include(entry => entry.Images)
                .Include(entry => entry.SellerCategories)
                .Include(entry => entry.Schedules)
                .Include(entry => entry.Addresses.Select(a => a.Region))
                .Include(entry => entry.Reviews.Select(review => review.ChildReviews))
                .FirstOrDefault(entry => entry.Domain == urlName || entry.UrlName == urlName);
                return seller;
            }
        }

        public List<Category> GetAllSellerCategories(string sellerUrl)
        {
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers
                    .Include(entry => entry.SellerCategories.Select(sc => sc.Category.Products))
                    .Include(entry => entry.SellerCategories.Select(sc => sc.Category.ParentCategory))
                    .Include(entry => entry.MappedCategories.Select(mc => mc.MappedParentCategory.Products))
                    .FirstOrDefault(entry => entry.UrlName == sellerUrl);
                var all = new List<Category>();
                var sellerCats = seller.SellerCategories.Where(entry => !entry.RootDisplay).Select(entry => entry.Category).Where(entry => entry.Products.Any() || entry.MappedCategories.Any()).ToList();
                var sellerRootCats = seller.SellerCategories.Where(entry => entry.RootDisplay).Select(entry => entry.Category).Where(entry => entry.Products.Any() || entry.MappedCategories.Any()).ToList();
                var sellerMappedCats = seller.MappedCategories.Where(entry => entry.MappedParentCategory != null && entry.IsActive).Select(entry => entry.MappedParentCategory).Where(entry => entry.Products.Any()).ToList();
                all.AddRange(sellerCats);
                all.AddRange(sellerMappedCats);
                foreach (var sellerRootCat in sellerRootCats)
                {
                    sellerRootCat.ParentCategory = null;
                    sellerRootCat.ParentCategoryId = null;
                }
                all.AddRange(sellerRootCats);
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
                all = all.Distinct(new CategoryComparer()).ToList();
                foreach (var cat in all)
                {
                    var sellerCat = seller.SellerCategories.FirstOrDefault(entry => entry.CategoryId == cat.Id);
                    if (sellerCat != null)
                    {
                        cat.Name = sellerCat.CustomName ?? cat.Name;
                        cat.ImageUrl = sellerCat.CustomImageUrl ?? cat.ImageUrl;
                    }
                }
                return all;
            }
        }

        public void ProcessCategories(List<SellerCategory> categories, string sellerId)
        {
            using (var db = new ApplicationDbContext())
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
        }

        public void ProcessAddresses(List<Address> addresses, string sellerId)
        {
            using (var db = new ApplicationDbContext())
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
        }
        public void ProcessShippingMethods(List<ShippingMethod> shippingMethods, string sellerId)
        {
            using (var db = new ApplicationDbContext())
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
}
