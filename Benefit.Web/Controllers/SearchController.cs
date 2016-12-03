using System.Linq;
using System.Web.Mvc;
using Benefit.DataTransfer.JSON;
using Benefit.Domain.Models.Search;
using Benefit.Services;

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
    }
}