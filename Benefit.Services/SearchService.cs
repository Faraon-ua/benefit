using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using Benefit.Common.Constants;
using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models.Search;
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
            var translitTerm = term.Translit();
            var productResult =
                db.Products.Include(entry => entry.Category).Include(entry => entry.Seller)
                .Where(entry => entry.IsActive && entry.Seller.IsActive)
                    .Select(
                        entry =>
                            entry.Name.ToLower() + " " + entry.SearchTags.ToLower() + " " +
                            entry.Category.Name.ToLower())
                    .Where(entry => entry.Contains(term)).ToList();
            var sellerResults =
                db.Sellers.Where(entry => entry.Name.ToLower().Contains(term) || entry.Name.ToLower().Contains(translitTerm))
                    .Select(entry => entry.Name)
                    .ToList()
                    .SelectMany(entry => entry.Split(new[] { ',', ' ', '"', '(', ')', '-' }))
                    .Where(entry => entry.ToLower().Contains(term) || entry.ToLower().Contains(translitTerm))
                    .Distinct()
                    .ToList();
            var words =
                productResult.Select(entry => entry.Split(new[] { ',', ' ', '"', '(', ')', '-' }))
                    .SelectMany(entry => entry)
                    .Where(entry => entry.Contains(term))
                    .GroupBy(entry => entry).Select(entry => new { entry.Key, Count = entry.Count() })
                   .OrderByDescending(entry => entry.Count).Select(entry => entry.Key).ToList();
            words.InsertRange(0, sellerResults);
            return words;
        }

        public SearchResult SearchProducts(string term, int skip, string searchSellerId = null, int take = ListConstants.DefaultTakePerPage, string categoryId = null)
        {
            var result = new SearchResult()
            {
                Term = term
            };
            term = term.ToLower();
            var translitTerm = term.Translit();
            var productsResult = db.Products.Include(entry => entry.Category).Include(entry => entry.Seller).Where(entry => entry.IsActive && entry.Seller.IsActive && entry.Seller.HasEcommerce);
            if (searchSellerId != null)
            {
                productsResult = productsResult.Where(entry => entry.SellerId == searchSellerId);
            }
            if (categoryId != null)
            {
                productsResult = productsResult.Where(entry => entry.CategoryId == categoryId);
            }
            var regionId = RegionService.GetRegionId();
            var productResult = productsResult.Search(entry => entry.Name,
                    entry => entry.SearchTags,
                    entry => entry.Category.Name
                ).Containing(term.Split(new[] { ' ' }))
                .ToRanked()
                .Where(entry => entry.Item.IsActive)
                .OrderByDescending(entry => entry.Hits)
                .Skip(skip)
                .Take(take + 1)
                .ToList();

            result.Products = productResult.Select(entry => entry.Item).ToList();

            if (searchSellerId == null)
            {
                var sellers =
                    db.Sellers
                        .Include(entry => entry.Addresses)
                        .Include(entry => entry.ShippingMethods)
                        .Include(entry => entry.ShippingMethods.Select(sh => sh.Region))
                        .Where(entry => entry.IsActive)
                        .Search(entry => entry.Name, entry => entry.SearchTags)
                        .Containing(term, translitTerm);

                result.CurrentRegionSellers =
                    sellers.Where(entry => entry.Addresses.Any(addr => addr.RegionId == regionId)).ToList();
                var currectRegionSellerIds = result.CurrentRegionSellers.Select(entry => entry.Id).ToList();
                result.Sellers = sellers.Where(entry => !currectRegionSellerIds.Contains(entry.Id)).ToList();
            }

            return result;
        }
    }
}
