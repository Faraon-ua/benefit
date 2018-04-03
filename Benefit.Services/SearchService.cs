﻿using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using Benefit.Common.Constants;
using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
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

        public SearchResult SearchProducts(string term, string options, int skip, string searchSellerId = null, int take = ListConstants.DefaultTakePerPage, string categoryId = null)
        {
            var result = new SearchResult()
            {
                Term = term
            };
            term = term.ToLower();
            var translitTerm = term.Translit();
            var productsResult = db.Products
                .Include(entry => entry.Category)
                .Include(entry => entry.Seller)
                .Where(entry => entry.IsActive && entry.Seller.IsActive && entry.Seller.HasEcommerce);
            if (searchSellerId != null)
            {
                productsResult = productsResult.Where(entry => entry.SellerId == searchSellerId);
            }
            if (categoryId != null)
            {
                productsResult = productsResult.Where(entry => entry.CategoryId == categoryId);
            }
            //var regionId = RegionService.GetRegionId();
            var productResult = productsResult.Search(entry => entry.Name,
                    entry => entry.SearchTags,
                    entry => entry.Category.Name
                ).Containing(term.Split(new[] { ' ' }))
                .ToRanked()
                .Where(entry => entry.Item.IsActive)
                .OrderByDescending(entry => entry.Hits)
                .Select(entry => entry.Item);

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
                        case "seller":
                            var sellerIds =
                                db.Sellers.Where(entry => optionValues.Contains(entry.UrlName))
                                    .Select(entry => entry.Id);
                            productResult = productResult.Where(entry => sellerIds.Contains(entry.SellerId));
                            break;
                        case "vendor":
                            productResult = productResult.Where(entry => optionValues.Contains(entry.Vendor));
                            break;
                        case "country":
                            productResult = productResult.Where(entry => optionValues.Contains(entry.OriginCountry));
                            break;
                        default:
                            productResult =
                                productResult.Where(
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

            //fetch parameters 
            var sellers = (from seller in productResult.Select(entry => entry.Seller)
                           group seller by seller.Id into groupResult
                           select new
                           {
                               Count = groupResult.Distinct().Count(),
                               Seller = (from sel in productResult.Select(entry => entry.Seller)
                                         where sel.Id == groupResult.Key
                                         select sel).FirstOrDefault()
                           }).OrderByDescending(y => y.Count).Select(entry => entry.Seller).ToList();
            var sellerFilters = sellers.Select(entry => new ProductParameterValue()
            {
                ParameterValue = entry.Name,
                ParameterValueUrl = entry.UrlName
            }).OrderBy(entry => entry.ParameterValue).ToList();
            result.ProductParameters.Add(new ProductParameter()
            {
                Name = "Постачальник",
                UrlName = "seller",
                Type = typeof(string).ToString(),
                ProductParameterValues = sellerFilters
            });

            var vendors = (from product in productResult
                           group product by product.Vendor into groupResult
                           select new
                           {
                               Count = groupResult.Distinct().Count(),
                               Vendor = groupResult.Key
                           }).OrderByDescending(y => y.Count).Select(entry => entry.Vendor).Where(entry => entry != null).ToList();
            var vendorFilters = vendors.Select(entry => new ProductParameterValue()
            {
                ParameterValue = entry,
                ParameterValueUrl = entry
            }).OrderBy(entry => entry.ParameterValue).ToList();
            result.ProductParameters.Add(new ProductParameter()
            {
                Name = "Виробник",
                UrlName = "vendor",
                Type = typeof(string).ToString(),
                ProductParameterValues = vendorFilters
            });

            var originCountries = (from product in productResult
                                   group product by product.OriginCountry into groupResult
                                   select new
                                   {
                                       Count = groupResult.Distinct().Count(),
                                       OriginCountry = groupResult.Key
                                   }).OrderByDescending(y => y.Count).Select(entry => entry.OriginCountry).Where(entry => entry != null).ToList();
            var countryFilters = originCountries.Select(entry => new ProductParameterValue()
            {
                ParameterValue = entry,
                ParameterValueUrl = entry
            }).OrderBy(entry => entry.ParameterValue).ToList();
            result.ProductParameters.Add(new ProductParameter()
            {
                Name = "Країна виробник",
                UrlName = "country",
                Type = typeof(string).ToString(),
                ProductParameterValues = countryFilters
            });

            result.Products = productResult.Skip(skip)
                .Take(take + 1)
                .ToList();

            //if (searchSellerId == null)
            //{
            //    var sellers =
            //        db.Sellers
            //            .Include(entry => entry.Addresses)
            //            .Include(entry => entry.ShippingMethods)
            //            .Include(entry => entry.ShippingMethods.Select(sh => sh.Region))
            //            .Where(entry => entry.IsActive)
            //            .Search(entry => entry.Name, entry => entry.SearchTags)
            //            .Containing(term, translitTerm);

            //    result.CurrentRegionSellers =
            //        sellers.Where(entry => entry.Addresses.Any(addr => addr.RegionId == regionId)).ToList();
            //    var currectRegionSellerIds = result.CurrentRegionSellers.Select(entry => entry.Id).ToList();
            //    result.Sellers = sellers.Where(entry => !currectRegionSellerIds.Contains(entry.Id)).ToList();
            //}

            return result;
        }
    }
}
