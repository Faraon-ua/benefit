using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Services.Domain;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;

namespace Benefit.Web.Controllers
{
    public class CatalogController : BaseController
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

        [FetchCategories]
        [FetchLastNews]
        public ActionResult Index(string categoryUrl, string options)
        {
            if (options != null && options.Contains("sellers"))
            {
                var sellers = SellerService.GetSellersCatalog(categoryUrl, options);
                if (sellers == null)
                {
                    return HttpNotFound();
                }
                sellers.Breadcrumbs = new BreadCrumbsViewModel()
                {
                    Categories = CategoriesService.GetBreadcrumbs(urlName: categoryUrl)
                };
                return View("SellersCatalog", sellers);
            }
            var catsModel = CategoriesService.GetCategoriesCatalog(categoryUrl);
            if (catsModel != null)
            {
                return View("CategoriesCatalog", catsModel);
            }

            var catalog = SellerService.GetSellerProductsCatalog(null, categoryUrl, options);
            return View("ProductsCatalog", catalog);
            //var sellers = CategoriesService.GetCategorySellers(categoryUrl);
            //if (sellers == null) return HttpNotFound();
            //return View("SellersCatalog", sellers);
        }

        public ActionResult GetProducts(string categoryId, string sellerId, string options, int page)
        {
            var skip = page * ListConstants.DefaultTakePerPage;
            var products = SellerService.GetSellerCatalogProducts(sellerId, categoryId, options, skip, ListConstants.DefaultTakePerPage, false).Products;
            var productsHtml = string.Join("", products.Take(ListConstants.DefaultTakePerPage).Select(entry => ControllerContext.RenderPartialToString("_ProductPartial", new ProductPartialViewModel
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