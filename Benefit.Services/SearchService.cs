using Benefit.Common.Constants;
using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Search;
using NinjaNye.SearchExtensions;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Benefit.Services
{
    public class SearchService
    {
        public ApplicationDbContext db = new ApplicationDbContext();

        public List<string> SearchKeyWords(string term, string sellerId = null)
        {
            term = term.ToLower();
            var translitTerm = term.Translit();
            var products = db.Products.Include(entry => entry.Category).Include(entry => entry.Seller)
                .Where(entry => entry.IsActive && entry.AvailabilityState != ProductAvailabilityState.NotInStock &&
                                entry.Seller.IsActive && entry.Category.IsActive);
            if (!string.IsNullOrEmpty(sellerId))
            {
                products = products.Where(entry => entry.SellerId == sellerId);
            }
            var productResult =
                products.Select(
                        entry =>
                            entry.Name.ToLower() + " " + entry.SearchTags.ToLower() + " " +
                            entry.Category.Name.ToLower())
                    .Where(entry => entry.Contains(term)).ToList();

            List<string> sellerResults = null;
            if (!string.IsNullOrEmpty(sellerId))
            {
                sellerResults =
                   db.Sellers.Where(entry =>
                           entry.Name.ToLower().Contains(term) || entry.Name.ToLower().Contains(translitTerm))
                       .Select(entry => entry.Name)
                       .ToList()
                       .SelectMany(entry => entry.Split(new[] { ',', ' ', '"', '(', ')', '-' }))
                       .Where(entry => entry.ToLower().Contains(term) || entry.ToLower().Contains(translitTerm))
                       .Distinct()
                       .ToList();
            }

            var words =
                productResult.Select(entry => entry.Split(new[] { ',', ' ', '"', '(', ')', '-' }))
                    .SelectMany(entry => entry)
                    .Where(entry => entry.Contains(term))
                    .GroupBy(entry => entry).Select(entry => new { entry.Key, Count = entry.Count() })
                   .OrderByDescending(entry => entry.Count).Select(entry => entry.Key).ToList();
            if (!string.IsNullOrEmpty(sellerId))
            {
                words.InsertRange(0, sellerResults);
            }
            return words;
        }

        public SearchResult SearchProducts(string term, string options, int skip, string searchSellerId = null, int take = ListConstants.DefaultTakePerPage)
        {
            var result = new SearchResult()
            {
                Term = term
            };
            term = term.ToLower();
            var translitTerm = term.Translit();
            var productsResult = db.Products
                .Include(entry => entry.Seller)
                .Include(entry => entry.Images)
                .Where(entry => entry.IsActive && entry.Seller.IsActive && entry.Seller.HasEcommerce && entry.Category.IsActive);
            if (searchSellerId != null)
            {
                productsResult = productsResult.Where(entry => entry.SellerId == searchSellerId);
            }
            //var regionId = RegionService.GetRegionId();
            var productResult = productsResult
                .Search(entry => entry.Name,
                    entry => entry.SearchTags,
                    entry => entry.Description,
                    entry => entry.ShortDescription
                ).Containing(term.Split(' '))
                .ToRanked()
                .Where(entry => entry.Item.IsActive)
                .OrderBy(entry => entry.Item.AvailabilityState)
                .ThenByDescending(entry => entry.Item.Images.Any())
                .ThenBy(entry => entry.Hits)
                .ThenBy(entry => entry.Item.SKU)
                .Select(entry => entry.Item);

            if (options != null)
            {
                var optionSegments = options.Split(';');
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
                        case "category":
                            var categories =
                                db.Categories
                                    .Include(entry=>entry.MappedCategories)
                                    .Where(entry => optionValues.Contains(entry.UrlName));
                            var mappedIds = categories.SelectMany(entry => entry.MappedCategories.Select(mc => mc.Id))
                                .ToList();
                            mappedIds.AddRange(categories.Select(entry=>entry.Id));
                            productResult = productResult.Where(entry => mappedIds.Contains(entry.CategoryId));
                            break;
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
            var categoryIdResults = productResult.GroupBy(entry => entry.CategoryId, (key, g) =>
            new
            {
                Count = g.Distinct().Count(),
                CategoryId = key
            }).Where(entry => entry.CategoryId != null).ToList();
            var catIds = categoryIdResults.Select(entry => entry.CategoryId).ToList();
            var categoriesInResults = db.Categories
                .Include(entry => entry.MappedParentCategory.ParentCategory)
                .Include(entry => entry.ParentCategory)
                .Where(entry => catIds.Contains(entry.Id)).ToList();
            var categoryResults = categoryIdResults.Select(entry => new
            {
                entry.Count,
                Category = categoriesInResults.FirstOrDefault(cat => cat.Id == entry.CategoryId)
            }).ToList();
            categoryResults.ForEach(entry =>
                {
                    if (entry.Category.MappedParentCategory != null)
                    {
                        entry.Category.ParentCategoryId = entry.Category.MappedParentCategoryId;
                        entry.Category.ParentCategory = entry.Category.MappedParentCategory;
                    }
                });

            var groupedCategoryResults = categoryResults.GroupBy(entry => entry.Category.ParentCategory, (key, g) =>
                  new
                  {
                      Parent = key,
                      Items = g.ToList()
                  }, new CategoryComparer())
            .ToList();

            var categoryFilters = groupedCategoryResults.Select(entry => new ProductParameterValue()
            {
                ParameterValue = entry.Parent.Name,
                ParameterValueUrl = entry.Parent.UrlName,
                Children = entry.Items.OrderByDescending(item => item.Count).Select(item => new ProductParameterValue()
                {
                    ParameterValue = item.Category.Name,
                    ParameterValueUrl = item.Category.UrlName,
                    ProductsCount = item.Count
                }).ToList()
            }).OrderBy(entry => entry.ParameterValue).ToList();
            result.ProductParameters.Add(new ProductParameter()
            {
                Name = "Категорія",
                UrlName = "category",
                Type = typeof(Category).ToString(),
                ProductParameterValues = categoryFilters,
                DisplayInFilters = true
            });

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
                ProductParameterValues = sellerFilters,
                DisplayInFilters = true
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
                ProductParameterValues = vendorFilters,
                DisplayInFilters = true
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
                ProductParameterValues = countryFilters,
                DisplayInFilters = true
            });

            result.PagesCount = (productResult.Count() - 1) / ListConstants.DefaultTakePerPage + 1;
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
            result.PagesCount = (productResult.Count() - 1) / ListConstants.DefaultTakePerPage + 1;
            result.Products = productResult.Skip(skip)
                .Take(take + 1)
                .ToList();

            return result;
        }
    }
}
