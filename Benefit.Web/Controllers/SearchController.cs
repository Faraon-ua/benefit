using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.JSON;
using Benefit.DataTransfer.ViewModels;
using Benefit.DataTransfer.ViewModels.NavigationEntities;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Search;
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

        [FetchSeller]
        [FetchCategories]
        public ActionResult Index(string term, string options, string searchSellerId)
        {
            searchSellerId = searchSellerId == string.Empty ? null : searchSellerId;
            var result = SearchService.SearchProducts(term, options, 0, searchSellerId);
            result.SellerId = searchSellerId;
            if (!string.IsNullOrEmpty(searchSellerId))
            {
                return View("~/Views/SellerArea/ProductsCatalog.cshtml", new ProductsViewModel
                {
                    Items = result.Products,
                    Category = new Category()
                    {
                        Name = "Пошук",
                        Description = string.Format("Результати пошуку по запиту '{0}'", result.Term)
                    },
                    Breadcrumbs = new BreadCrumbsViewModel()
                    {
                        Categories = new Dictionary<Category, List<Category>>()
                        {
                            {
                                new Category()
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

        public ActionResult GetProducts(string term, string options, int page)
        {
            var result = SearchService.SearchProducts(term, options, ListConstants.DefaultTakePerPage * page);
            var productsHtml = string.Join("", result.Products.Select(entry => ControllerContext.RenderPartialToString("~/Views/Catalog/_ProductPartial.cshtml", new ProductPartialViewModel
            {
                Product = entry,
                CategoryUrl = entry.Category.UrlName,
                SellerUrl = entry.Seller.UrlName
            })));
            return Json(new { number = result.Products.Count, products = productsHtml }, JsonRequestBehavior.AllowGet);
        }
    }
}