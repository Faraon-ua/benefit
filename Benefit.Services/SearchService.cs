using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using NinjaNye.SearchExtensions;

namespace Benefit.Services
{
    public class SearchService
    {
        public ApplicationDbContext db = new ApplicationDbContext();

        public List<string> SearchKeyWords(string term, string categoryId = null)
        {
            term = term.ToLower();
            var result =
                db.Products.Include(entry => entry.Category)
                    .Select(entry => entry.Name.ToLower() + " " + entry.SearchTags.ToLower() + " " + entry.Category.Name.ToLower())
                    .Where(entry => entry.Contains(term))
                    .ToList();
            var words =
                result.Select(entry => entry.Split(new[] { ',', ' ' })).ToList()
                    .SelectMany(entry => entry).ToList()
                    .Where(entry => entry.Contains(term)).ToList()
                    .Distinct().ToList();
            return words;
        }

        public List<Product> SearchProducts(string term, int skip, int take = ListConstants.DefaultTakePerPage, string categoryId = null)
        {
            var productsResult = db.Products.Include(entry => entry.Category).Include(entry => entry.Seller);
            if (categoryId != null)
            {
                productsResult = productsResult.Where(entry => entry.CategoryId == categoryId);
            }
            var regionId = RegionService.GetRegionId();
            var result = productsResult.Search(entry => entry.Name,
                    entry => entry.SearchTags,
                    entry => entry.Category.Name
                ).Containing(term.Split(new[] { ' ' }))
                .ToRanked()
                .Where(entry => entry.Item.Seller.Addresses.Any(addr => addr.RegionId == regionId) ||
                             entry.Item.Seller.ShippingMethods.Select(sm => sm.Region.Id)
                                 .Contains(RegionConstants.AllUkraineRegionId))
                .Where(entry=>entry.Item.IsActive)
                .OrderByDescending(entry => entry.Hits)
                .Skip(skip)
                .Take(take + 1)
                .ToList();

            return result.Select(entry => entry.Item).ToList();
        }
    }
}
