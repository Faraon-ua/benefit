using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            categories = categories.Distinct(new SellerCategoryComparer()).ToList();
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
