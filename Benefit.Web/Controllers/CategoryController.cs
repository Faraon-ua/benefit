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
            return PartialView("~/views/catalog/_ProductFilters.cshtml", parameters);
        }
    }
}