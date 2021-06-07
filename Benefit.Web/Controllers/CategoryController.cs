using Benefit.Domain.Models;
using Benefit.Services.Domain;
using Benefit.Web.Filters;
using System.Collections.Generic;
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
            ICollection<ProductParameter> parameters = null;
            if (categoryId != null)
            {
                parameters = catalogService.GetProductParameters(categoryId, seller == null ? null : seller.Id);
            }
            if (seller != null && seller.HasEcommerce)
            {
                var viewName = string.Format("~/views/sellerarea/{0}/_ProductFilters.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return PartialView(viewName, parameters);
            }
            return PartialView("~/views/catalog/_ProductFilters.cshtml", parameters);
        }
    }
}