using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels.Base;
using Benefit.DataTransfer.ViewModels.NavigationEntities;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benefit.Services.Domain
{
    public class CatalogService
    {
        ProductsDBContext productsDBContext;
        public CatalogService()
        {
            productsDBContext = new ProductsDBContext();
        }
        public ICollection<ProductParameter> GetProductParameters(string categoryId, string sellerId)
        {
            var ppContext = new ProductParametersDBContext();
            var catalogParams = productsDBContext.GetCatalogParams(categoryId, sellerId);
            var productParameters = ppContext.Get(categoryId, sellerId);
            var generalParams = FetchGeneralProductParameters(catalogParams, sellerId == null);
            var productParametersInItems = catalogParams.ProductParameters.Split(',').Distinct().Select(entry => entry.ToLower().Replace("&", string.Empty)).ToList();
            productParametersInItems.AddRange(generalParams.SelectMany(entry => entry.ProductParameterValues).Select(entry => entry.ParameterValueUrl).ToList());
            productParameters.InsertRange(0, generalParams);
            var productParameterNames = productParameters.Where(entry => !entry.SkipCheckInItems).Select(entry => entry.UrlName).Distinct().ToList();
            foreach (var productParameterName in productParameterNames)
            {
                var parameters = productParameters.Where(entry => entry.UrlName == productParameterName).ToList();
                var parameter = parameters.First();
                parameter.ProductParameterValues =
                    parameters.SelectMany(entry => entry.ProductParameterValues)
                        .Distinct(new ProductParameterValueComparer())
                        .ToList();
                var childValues = parameter.ChildProductParameters
                    .SelectMany(entry => entry.ProductParameterValues).ToList();
                (parameter.ProductParameterValues as List<ProductParameterValue>).AddRange(childValues);
                //if (options == null || !options.Contains(parameter.UrlName))
                //{
                //    parameter.ProductParameterValues.Where(
                //            entry => productParametersInItems.Contains(entry.ParameterValueUrl)).ToList()
                //        .ForEach(entry => entry.Enabled = true);
                //}
                //else
                //{
                //    parameter.ProductParameterValues.ToList().ForEach(entry => entry.Enabled = true);
                //}

                //result.ProductParameters.Add(parameter);
                parameter.ProductParameterValues.ToList().ForEach(entry => entry.Enabled = true);
            }
            return productParameters;
        }
        public ProductsViewModel GetSellerProductsCatalog(string sellerId, string categoryId, string userId, string options)
        {
            var result = new ProductsViewModel();
            var where = new StringBuilder();
            var sqlParams = new List<SqlParameter>();
            var whereProductParameters = new Dictionary<string, List<string>>();
            string orderby = null;
            var sort = ProductSortOption.Rating;
            result.Page = 1;
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
                    //var dbOptionKey = db.ProductParameters.Include(entry => entry.ChildProductParameters)
                    //    .FirstOrDefault(entry => entry.UrlName == optionKey && entry.CategoryId == categoryId);
                    //var optionKeysWithChildren = dbOptionKey == null
                    //    ? new List<string>()
                    //    : dbOptionKey.ChildProductParameters
                    //        .Select(entry => entry.UrlName).Distinct().ToList();
                    //optionKeysWithChildren.Add(optionKey);
                    var optionValues = optionKeyValue.Last().Split(',').ToList();
                    switch (optionKey)
                    {
                        case "sort":
                            sort = (ProductSortOption)Enum.Parse(typeof(ProductSortOption), optionValues.First());
                            break;
                        case "page":
                            result.Page = int.Parse(optionValues[0]);
                            break;
                        case "vendor":
                            var vendors = string.Join(",", optionValues.Select((entry, index) => "@vendor" + index));
                            var vendorParams = optionValues.Select((entry, index) => new SqlParameter
                            {
                                ParameterName = "@vendor" + index,
                                SqlDbType = SqlDbType.NVarChar,
                                Direction = ParameterDirection.Input,
                                Value = optionValues[index]
                            });
                            sqlParams.AddRange(vendorParams);
                            where.Append(string.Format(" and p.Vendor in ({0})", vendors));
                            break;
                        case "seller":
                            var sellers = string.Join(",", optionValues.Select((entry, index) => "@seller" + index));
                            var sellerParams = optionValues.Select((entry, index) => new SqlParameter
                            {
                                ParameterName = "@seller" + index,
                                SqlDbType = SqlDbType.NVarChar,
                                Direction = ParameterDirection.Input,
                                Value = optionValues[index]
                            });
                            sqlParams.AddRange(sellerParams);
                            where.Append(string.Format(" and s.UrlName in ({0})", sellers));
                            break;
                        case "available":
                            if (optionValues.Any())
                            {
                                if (optionValues.First() == "available")
                                {
                                    where.Append(string.Format(" and p.AvailabilityState in ({0})", string.Join(",",
                                        new int[] {
                                            (int)ProductAvailabilityState.Available,
                                            (int)ProductAvailabilityState.AlwaysAvailable,
                                            (int)ProductAvailabilityState.OnDemand,
                                            (int)ProductAvailabilityState.Ending
                                        })));
                                }
                                if (optionValues.First() == "notavailable")
                                {
                                    where.Append(string.Format(" and p.AvailabilityState in ({0})", string.Join(",",
                                            new int[] {
                                                (int)ProductAvailabilityState.NotInStock
                                            })));
                                }
                            }
                            break;
                        case "country":
                            var countries = string.Join(",", optionValues.Select((entry, index) => "@country" + index));
                            var countryParams = optionValues.Select((entry, index) => new SqlParameter
                            {
                                ParameterName = "@country" + index,
                                SqlDbType = SqlDbType.NVarChar,
                                Direction = ParameterDirection.Input,
                                Value = optionValues[index]
                            });
                            sqlParams.AddRange(countryParams);
                            where.Append(string.Format(" and p.Vendor in ({0})", countries));
                            break;
                        case "price":
                            var prices = optionValues[0].Split('-');
                            var lowerPrice = int.Parse(prices[0]);
                            var upperPrice = int.Parse(prices[1]);
                            where.Append(string.Format(" and Price > {0} and Price < {1}", lowerPrice, upperPrice));
                            break;
                        default:
                            whereProductParameters.Add(optionKey, optionValues);
                            break;
                    }
                }
            }
            if (whereProductParameters.Any())
            {
                where.Append(" and p.Id in (");
                for (var i = 0; i < whereProductParameters.Count; i++)
                {
                    var pValCount = 0;
                    where.Append(string.Format(@"SELECT distinct ProductId
                                    FROM ProductParameterProducts ppp, ProductParameters pp
                                    where ppp.ProductParameterId = pp.Id and ppp.StartValue in ({0}) and 
                                    pp.UrlName = @productParam{1}",
                                    string.Join(",", whereProductParameters.ElementAt(i).Value.Select(entry => string.Format("@pVal{0}{1}", i, pValCount++))),
                                    i));
                    if (i < whereProductParameters.Count - 1)
                    {
                        where.Append(" INTERSECT ");
                    }
                    sqlParams.Add(new SqlParameter
                    {
                        ParameterName = "@productParam" + i,
                        SqlDbType = SqlDbType.NVarChar,
                        Direction = ParameterDirection.Input,
                        Value = whereProductParameters.ElementAt(i).Key
                    });
                    for (var j = 0; j < whereProductParameters.ElementAt(i).Value.Count; j++)
                    {
                        sqlParams.Add(new SqlParameter
                        {
                            ParameterName = string.Format("@pVal{0}{1}", i, j),
                            SqlDbType = SqlDbType.NVarChar,
                            Direction = ParameterDirection.Input,
                            Value = whereProductParameters.ElementAt(i).Value[j]
                        });
                    }
                }
                where.Append(")");
            }
            switch (sort)
            {
                case ProductSortOption.Rating:
                    orderby = "p.[Order], IsNewProduct desc, AvailabilityState, HasImages desc, p.AddedOn, p.AvarageRating, p.SKU";
                    break;
                case ProductSortOption.Order:
                    orderby = "HasImages desc, p.SKU";
                    break;
                case ProductSortOption.NameAsc:
                    orderby = "p.Name, p.SKU";
                    break;
                case ProductSortOption.NameDesc:
                    orderby = "p.Name desc, p.SKU";
                    break;
                case ProductSortOption.PriceAsc:
                    orderby = "Price, p.SKU";
                    break;
                case ProductSortOption.PriceDesc:
                    orderby = "Price desc, p.SKU";
                    break;
            }

            //if (sellerId != null)
            //{
            //    var mappedCategories = db.Categories
            //        .Include(entry => entry.MappedCategories.Select(mc => mc.ProductParameters.Select(pp => pp.ProductParameterProducts)))
            //        .Where(
            //            entry =>
            //                entry.SellerId == sellerId && entry.MappedParentCategoryId == categoryId &&
            //                entry.IsActive).ToList();
            //    var mappedCategoriesParameters = mappedCategories
            //        .SelectMany(entry => entry.ProductParameters)
            //        .Where(entry => entry.DisplayInFilters).ToList();
            //    productParameters = productParameters.Union(mappedCategoriesParameters).ToList();
            //}
            if (options == null || (options != null && result.Page == 1))
            {
                result.PagesCount = (productsDBContext.GetCatalogCount(categoryId, sellerId, where.ToString(), sqlParams) - 1) / ListConstants.DefaultTakePerPage + 1;
            }
            result.Items = productsDBContext.GetCatalog(categoryId, sellerId, userId, where.ToString(), orderby, ListConstants.DefaultTakePerPage * (result.Page - 1), ListConstants.DefaultTakePerPage, sqlParams);
            return result;
        }

        public List<ProductParameter> FetchGeneralProductParameters(CatalogParams catalogParams, bool fetchSellers)
        {
            var productParametersList = new List<ProductParameter>();
            productParametersList.Add(new ProductParameter()
            {
                Name = "Ціна",
                UrlName = "price",
                Type = typeof(string).ToString(),
                SkipCheckInItems = true,
                DisplayInFilters = false,
                ProductParameterValues = new List<ProductParameterValue>
                {
                    new ProductParameterValue()
                    {
                        ParameterValue = catalogParams.MinPrice.ToString()
                    },
                    new ProductParameterValue()
                    {
                        ParameterValue = catalogParams.MaxPrice.ToString()
                    }
                }
            });
            var availableList = new List<ProductParameterValue>
            {
                new ProductParameterValue()
                {
                    ParameterValue = "Є в наявності",
                    ParameterValueUrl = "available"
                },
                new ProductParameterValue()
                {
                    ParameterValue = "Немає в наявності",
                    ParameterValueUrl = "notavailable"
                }
            };
            productParametersList.Add(new ProductParameter()
            {
                Name = "Наявність",
                UrlName = "available",
                Type = typeof(string).ToString(),
                DisplayInFilters = true,
                ProductParameterValues = availableList
            });
            if (fetchSellers)
            {
                var sellerNames = catalogParams.SellerNames.Split(',');
                var sellerUrls = catalogParams.SellerUrls.Split(',');
                var sellersParams = new List<ProductParameterValue>();
                for (var i = 0; i < sellerNames.Length; i++)
                {
                    sellersParams.Add(new ProductParameterValue()
                    {
                        ParameterValue = sellerNames[i],
                        ParameterValueUrl = sellerUrls[i]
                    });
                }
                sellersParams = sellersParams.OrderBy(entry => entry.ParameterValue).ToList();
                productParametersList.Add(new ProductParameter()
                {
                    Name = "Постачальник",
                    UrlName = "seller",
                    DisplayInFilters = true,
                    Type = typeof(string).ToString(),
                    ProductParameterValues = sellersParams
                });
            }

            if (catalogParams.Vendors != null)
            {
                var vendorParams =
                    catalogParams.Vendors.Split(',').Select(entry => new ProductParameterValue()
                    {
                        ParameterValue = entry,
                        ParameterValueUrl = entry.Replace("&", "")
                    }).OrderBy(entry => entry.ParameterValue).ToList();
                if (vendorParams.Count >= 1)
                {
                    productParametersList.Add(new ProductParameter()
                    {
                        Name = "Виробник",
                        UrlName = "vendor",
                        Type = typeof(string).ToString(),
                        DisplayInFilters = true,
                        ProductParameterValues = vendorParams
                    });
                }
            }

            if (catalogParams.OriginCountries != null)
            {
                var originCountryParams =
                 catalogParams.OriginCountries.Split(',').Select(entry => new ProductParameterValue()
                 {
                     ParameterValue = entry,
                     ParameterValueUrl = entry
                 }).OrderBy(entry => entry.ParameterValue).ToList();
                if (originCountryParams.Count >= 1)
                {
                    productParametersList.Add(new ProductParameter()
                    {
                        Name = "Країна виробник",
                        UrlName = "country",
                        Type = typeof(string).ToString(),
                        DisplayInFilters = true,
                        ProductParameterValues = originCountryParams
                    });
                }
            }
            return productParametersList;
        }
        public Dictionary<CategoryVM, List<CategoryVM>> GetBreadcrumbs(IEnumerable<CategoryVM> cachedCats, string categoryId = null, string urlName = null)
        {
            var result = new Dictionary<CategoryVM, List<CategoryVM>>();
            var category = cachedCats.FindByUrlIdRecursively(urlName, categoryId);
            if (category != null)
            {
                while (category.ParentCategory != null)
                {
                    var nextCats = category.ParentCategory.ChildCategories.Where(entry => entry.Id != category.Id).ToList();
                    result.Add(category, nextCats);
                    category = category.ParentCategory;
                }
            }
            if (category != null)
            {
                result.Add(category, new List<CategoryVM>());
            }
            result = result.Reverse().ToDictionary(x => x.Key, x => x.Value);
            return result;
        }

    }
}