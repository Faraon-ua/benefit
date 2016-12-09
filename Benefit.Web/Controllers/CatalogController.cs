using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Services.Domain;
using Benefit.Web.Helpers;


namespace Benefit.Web.Controllers
{
    public class CatalogController : Controller
    {
        CategoriesService CategoriesService { get; set; }
        SellerService SellerService { get; set; }

        public CatalogController()
        {
            CategoriesService = new CategoriesService();
            SellerService = new SellerService();
        }
        [OutputCache(Duration = CacheConstants.OutputCacheLength)]
        public ActionResult GetBaseCategories()
        {
            var categories = CategoriesService.GetBaseCategories();
            return PartialView("_BaseCategoriesPartial", categories);
        }

        public ActionResult Index(string id)
        {
            var products = CategoriesService.GetCategoryProducts(id);
            if (products != null)
            {
                return View("ProductsCatalog", products);
            }
            var sellers = CategoriesService.GetCategorySellers(id);
            if (sellers == null) return HttpNotFound();
            return View("SellersCatalog", sellers);
        }

        public ActionResult GetProducts(string categoryId, string sellerId, int skip, int take = ListConstants.DefaultTakePerPage)
        {
            var products = SellerService.GetSellerCatalogProducts(sellerId, categoryId, skip, take);
            var productsHtml = string.Join("", products.Select(entry => ControllerContext.RenderPartialToString("_ProductPartial",new ProductPartialViewModel
            {
                Product = entry,
                CategoryUrl = entry.Category.UrlName,
                SellerUrl = entry.Seller.UrlName
            })));
            return Json(new { number = products.Count, products = productsHtml }, JsonRequestBehavior.AllowGet);
        }
    }
}