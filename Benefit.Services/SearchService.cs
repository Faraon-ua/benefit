using Benefit.Common.Constants;
using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Search;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CategoryProductsCount = Benefit.Domain.Models.Search.CategoryProductsCount;

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
            term = Regex.Replace(term.ToLower(), "[^а-яА-Я0-9a-zA-Zії ]+", "");
            var fullTextTerm = term;
            var words = term.Split(' ');
            if (words.Length > 1)
            {
                var searchTermBuilder = new StringBuilder();
                for (var i = 0; i < words.Length; i++)
                {
                    if (i < words.Length - 1)
                    {
                        searchTermBuilder.AppendFormat("\"{0}\" AND ", words[i]);
                    }
                    else
                    {
                        searchTermBuilder.AppendFormat("\"{0}\"", words[i]);
                    }
                }

                fullTextTerm = searchTermBuilder.ToString();
            }
            //var regionId = RegionService.GetRegionId();
            var query = string.Format(@"
            Select SearchResults.* from
            (Select top 300 Id, Sum(Rank) as Rank From
                (SELECT Id, Name, 5 as Rank
                    FROM Products
                    where Name like N'{0}%' AND IsActive = 1
                    union
                    SELECT Id, Name, 4 as Rank
                    FROM Products
                    where contains(Name, N'{1}') AND IsActive = 1 
                    union
                    SELECT Id, SearchTags, 3 as Rank
                    FROM Products
                    where Name like N'%{0}%' AND IsActive = 1
                    union
                    SELECT Id, Name, 2 as Rank
                    FROM Products
                    where SearchTags like N'%{0}%' AND IsActive = 1
                    union
                    SELECT Id, Description, 1 as Rank
                    FROM Products
                    where Description like N'%{0}%' AND IsActive = 1
                ) as RankedResults
                GROUP BY Id
            order by rank desc
                ) as SearchResults
            ", term, fullTextTerm);
            var rankedSearchResults = db.Database.SqlQuery<RankedSqlResult>(query).ToDictionary(x => x.Id, x => x.Rank);
            var searchedIds = rankedSearchResults.Select(entry => entry.Key).ToList();
            var productResults = db.Products
                .Include(entry => entry.Seller.ShippingMethods.Select(sh => sh.Region))
                .Include(entry => entry.Seller)
                .Include(entry => entry.Images)
                .Include(entry => entry.Category)
                .Where(entry => searchedIds.Contains(entry.Id)
                                && entry.Seller.IsActive
                                && entry.Seller.HasEcommerce
                                && entry.Category.IsActive
                                && ((entry.Category.IsSellerCategory && entry.Category.MappedParentCategoryId != null) || !entry.Category.IsSellerCategory));
            if (searchSellerId != null)
            {
                productResults = productResults.Where(entry => entry.SellerId == searchSellerId);
            }

            var productResult = productResults.ToList();
            productResult.ForEach(entry => entry.SearchRank = rankedSearchResults[entry.Id]);
            productResult = productResult.OrderByDescending(entry => entry.SearchRank)
            .ThenBy(entry => entry.AvailabilityState)
            .ThenByDescending(entry => entry.Images.Any())
            .ThenBy(entry => entry.SKU).ToList();

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
            var categoryResults = categoryIdResults.Select(entry => new CategoryProductsCount()
            {
                Count = entry.Count,
                Category = categoriesInResults.FirstOrDefault(cat => cat.Id == entry.CategoryId)
            }).ToList();

            for (var i = 0; i < categoryResults.Count; i++)
            {
                if (categoryResults[i].Category.MappedParentCategory != null)
                {
                    if (catIds.Contains(categoryResults[i].Category.MappedParentCategoryId))
                    {
                        categoryResults[i] = null;
                    }
                    else
                    {
                        categoryResults[i].Category.ParentCategoryId = categoryResults[i].Category.MappedParentCategory.ParentCategoryId;
                        categoryResults[i].Category.ParentCategory = categoryResults[i].Category.MappedParentCategory.ParentCategory;
                        categoryResults[i].Category = categoryResults[i].Category.MappedParentCategory;
                    }
                }
            }

            categoryResults = categoryResults.Where(entry => entry != null).ToList();

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
                        case "page":
                            var page = int.Parse(optionValues[0]);
                            productResult = productResult.Skip(ListConstants.DefaultTakePerPage * (page - 1)).ToList();
                            break;
                        case "category":
                            var categories =
                                db.Categories
                                    .Include(entry => entry.MappedCategories)
                                    .Where(entry => optionValues.Contains(entry.UrlName));
                            var mappedIds = categories.SelectMany(entry => entry.MappedCategories.Select(mc => mc.Id))
                                .ToList();
                            mappedIds.AddRange(categories.Select(entry => entry.Id));
                            productResult = productResult.Where(entry => mappedIds.Contains(entry.CategoryId)).ToList();
                            break;
                        case "seller":
                            var sellerIds =
                                db.Sellers.Where(entry => optionValues.Contains(entry.UrlName))
                                    .Select(entry => entry.Id);
                            productResult = productResult.Where(entry => sellerIds.Contains(entry.SellerId)).ToList();
                            break;
                        case "vendor":
                            productResult = productResult.Where(entry => optionValues.Contains(entry.Vendor)).ToList();
                            break;
                        case "country":
                            productResult = productResult.Where(entry => optionValues.Contains(entry.OriginCountry)).ToList();
                            break;
                        default:
                            productResult =
                                productResult.Where(
                                entry =>
                                    entry.ProductParameterProducts.Any(pr => pr.ProductParameter.UrlName == optionKey) &&
                                    optionValues.Any(
                                        optValue =>
                                            entry.ProductParameterProducts.Select(pr => pr.StartValue)
                                                .Contains(optValue))).ToList();
                            break;
                    }
                }
            }

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
