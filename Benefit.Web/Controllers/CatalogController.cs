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

        public ActionResult Index(string categoryUrl, string options)
        {
            var catalog = SellerService.GetSellerCatalog(null, categoryUrl, options);
            if (catalog.Items.Any())
            {
                return View("ProductsCatalog", catalog);
            }
            var sellers = CategoriesService.GetCategorySellers(categoryUrl);
            if (sellers == null) return HttpNotFound();
            return View("SellersCatalog", sellers);
        }

        public ActionResult GetProducts(string categoryId, string sellerId, string options, int skip, int take = ListConstants.DefaultTakePerPage)
        {
            var products = SellerService.GetSellerCatalogProducts(sellerId, categoryId, options, skip, take, false).Products;
            var productsHtml = string.Join("", products.Select(entry => ControllerContext.RenderPartialToString("_ProductPartial",new ProductPartialViewModel
            {
                Product = entry,
                CategoryUrl = entry.Category.UrlName,
                SellerUrl = entry.Seller.UrlName,
                AvailableForPurchase = entry.AvailableForPurchase(RegionService.GetRegionId())
            })));
            return Json(new { number = products.Count, products = productsHtml }, JsonRequestBehavior.AllowGet);
        }
    }
}