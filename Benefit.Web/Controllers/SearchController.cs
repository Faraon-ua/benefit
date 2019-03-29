using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.JSON;
using Benefit.DataTransfer.ViewModels;
using Benefit.DataTransfer.ViewModels.NavigationEntities;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;

namespace Benefit.Web.Controllers
{
    public class SearchController : Controller
    {
        SearchService SearchService = new SearchService();
        public ActionResult SearchWords(string query, string sellerId = null)
        {
            var searchResult = SearchService.SearchKeyWords(query, sellerId);
            var result = new AutocompleteSearch
            {
                query = query,
                suggestions = searchResult.Select(entry => new ValueData()
                {
                    value = entry,
                    data = entry
                }).ToArray()
            };
            return Json(result,
                JsonRequestBehavior.AllowGet);
        }

        [FetchSeller(Order=0)]
        [FetchCategories(Order = 1)]
        public ActionResult Index(string term, string options, string searchSellerId)
        {
            searchSellerId = searchSellerId == string.Empty ? null : searchSellerId;
            var result = SearchService.SearchProducts(term, options, 0, searchSellerId);
            result.SellerId = searchSellerId;
            if (!string.IsNullOrEmpty(searchSellerId))
            {
                var seller = ViewBag.Seller as Seller;
                var viewName = string.Format("~/Views/SellerArea/{0}/ProductsCatalog.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return View(viewName, new ProductsViewModel
                {
                    Items = result.Products,
                    PagesCount = result.PagesCount,
                    ProductParameters = result.ProductParameters,
                    Category = new CategoryVM()
                    {
                        Name = "Пошук: " + term,
                        UrlName = "search",
                        Description = string.Format("Результати пошуку по запиту '{0}'", result.Term)
                    },
                    Breadcrumbs = new BreadCrumbsViewModel()
                    {
                        Categories = new Dictionary<CategoryVM, List<CategoryVM>>()
                        {
                            {
                                new CategoryVM()
                                {
                                    Name = "Пошук"
                                },
                                null
                            }
                        }
                    }
                });
            }
            return View(result);
        }

        public ActionResult GetProducts(string term, string options, int page, SellerEcommerceTemplate? layout)
        {
            var result = SearchService.SearchProducts(term, options, ListConstants.DefaultTakePerPage * page);
            var templateName = "~/Views/Catalog/_ProductPartial.cshtml";
            if (layout.HasValue)
            {
                templateName = string.Format("~/Views/SellerArea/{0}/_ProductPartial.cshtml", layout.ToString());
            }
            var productsHtml = string.Join("", result.Products.Take(ListConstants.DefaultTakePerPage).Select(entry => ControllerContext.RenderPartialToString(templateName, entry)));
            return Json(new { number = result.Products.Count, products = productsHtml }, JsonRequestBehavior.AllowGet);
        }
    }
}