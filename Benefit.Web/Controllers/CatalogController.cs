using System.Web.Mvc;
using Benefit.Services.Domain;

namespace Benefit.Web.Controllers
{
    public class CatalogController : Controller
    {
        public ActionResult Index(string id)
        {
            var categoriesService = new CategoriesService();
            var products = categoriesService.GetCategoryProducts(id);
            if (products != null)
            {
                return View("ProductsCatalog", products);
            }
            var sellers = categoriesService.GetCategorySellers(id);
            if (sellers == null) return HttpNotFound();
            return View("SellersCatalog", sellers);
        }
	}
}