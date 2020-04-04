using Benefit.Domain.Models;
using Benefit.Services.Domain;
using Benefit.Web.Filters;
using System.Web.Mvc;

namespace Benefit.Web.Controllers
{
    public class CategoryController : Controller
    {
        CatalogService catalogService;
        public CategoryController()
        {
            catalogService = new CatalogService();
        }
        [FetchSeller()]
        public ActionResult GetProductFilters(string categoryId)
        {
            var seller = ViewBag.Seller as Seller;
            var parameters = catalogService.GetProductParameters(categoryId, seller.Id);
            var viewPath = string.Format("~/views/catalog/_ProductFilters.cshtml",
                   seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default)
                   .ToString());
            return PartialView(viewPath, parameters);
        }

        //public ActionResult GetProducts(string categoryId, string sellerId, string options)
        //{
        //catalogService.GetSellerProductGetCatalogCountsCatalog(category, cachedCats, seller.Id, categoryUrl, User.Identity.GetUserId(), options);

        //}
    }
}