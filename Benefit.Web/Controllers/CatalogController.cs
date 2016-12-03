using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Services.Domain;
using Benefit.Web.Helpers;


namespace Benefit.Web.Controllers
{
    public class CatalogController : Controller
    {
        CategoriesService CategoriesService { get; set; }

        public CatalogController()
        {
            CategoriesService = new CategoriesService();
        }
        [OutputCache(Duration = CacheConstants.OutputCacheLength)]
        public ActionResult GetBaseCategories()
        {
            var categories = CategoriesService.GetBaseCategories();
            return PartialView("_BaseCategoriesPartial", categories);
        }

        public ActionResult Index(string id)
        {
            var products = CategoriesService.GetCategoryProducts(id, 0);
            if (products != null)
            {
                return View("ProductsCatalog", products);
            }
            var sellers = CategoriesService.GetCategorySellers(id);
            if (sellers == null) return HttpNotFound();
            return View("SellersCatalog", sellers);
        }

        public ActionResult GetProducts(string categoryId, int skip, int take = ListConstants.DefaultTakePerPage)
        {
            var products = CategoriesService.GetCategoryProductsOnly(categoryId, skip, take);
            var productsHtml = string.Join("", products.Select(entry => ControllerContext.RenderPartialToString("_ProductPartial", entry)));
            return Json(new { number = products.Count, products = productsHtml }, JsonRequestBehavior.AllowGet);
        }
    }
}