using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.JSON;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.Models.Search;
using Benefit.Services;
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

        public ActionResult Index(string term)
        {
            var products = SearchService.SearchProducts(term, 0);
            var result = new SearchResult
            {
                Term = term,
                Products = products
            };
            return View(result);
        }

        public ActionResult GetProducts(string term, int skip, int take = ListConstants.DefaultTakePerPage)
        {
            var products = SearchService.SearchProducts(term, skip);
            var productsHtml = string.Join("", products.Select(entry => ControllerContext.RenderPartialToString("~/Views/Catalog/_ExpandableProductPartial.cshtml", new ProductPartialViewModel
            {
                Product = entry,
                CategoryUrl = entry.Category.UrlName,
                SellerUrl = entry.Seller.UrlName
            })));
            return Json(new { number = products.Count, products = productsHtml }, JsonRequestBehavior.AllowGet);
        }
    }
}