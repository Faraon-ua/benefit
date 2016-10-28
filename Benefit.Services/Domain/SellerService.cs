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
                .Include(entry=>entry.SellerCategories)
                .Include(entry=>entry.Schedules)
                .Include(entry=>entry.Addresses)
                .Include(entry=>entry.ShippingMethods)
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
    }
}
