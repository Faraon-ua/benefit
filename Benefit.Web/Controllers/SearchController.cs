using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.JSON;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.Models.Search;
using Benefit.Services;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;

namespace Benefit.Web.Controllers
{
    public class SearchController : Controller
    {
        SearchService SearchService = new SearchService();
        public ActionResult SearchWords(string query, string categoryId = null)
        {
            var searchResult = SearchService.SearchKeyWords(query, categoryId);
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

        [FetchCategories]
        public ActionResult Index(string term, string searchSellerId = null)
        {
            searchSellerId = searchSellerId == string.Empty ? null : searchSellerId;
            var result = SearchService.SearchProducts(term, 0, searchSellerId);
            result.SellerId = searchSellerId;
            return View(result);
        }

        public ActionResult GetProducts(string term, int page, int take = ListConstants.DefaultTakePerPage)
        {
            var result = SearchService.SearchProducts(term, ListConstants.DefaultTakePerPage * page);
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