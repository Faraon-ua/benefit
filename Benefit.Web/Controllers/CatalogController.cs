using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Services.Domain;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;
using Benefit.DataTransfer.ViewModels.NavigationEntities;
using Benefit.Domain.Models;
using Benefit.Web.Models;

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

        [FetchSeller(Order=0)]
        [FetchCategories(Order = 1)]
        [FetchLastNews]
        public ActionResult Index(string categoryUrl, string options)
        {
            var cachedCats = ViewBag.Categories as List<Category>;
            if (categoryUrl == "postachalnuky")
            {
                var sellers = SellerService.GetSellersCatalog(options);
                if (sellers == null)
                {
                    throw new HttpException(404, "Not found");
                }
                sellers.Breadcrumbs = new BreadCrumbsViewModel()
                {
                    Categories = CategoriesService.GetBreadcrumbs(cachedCats, urlName: categoryUrl)
                };
                return View("SellersCatalog", sellers);
            }

            CategoriesViewModel catsModel = CategoriesService.GetCategoriesCatalog(cachedCats, categoryUrl, ViewBag.SellerUrl);
            if (catsModel == null)
            {
                throw new HttpException(404, "Not found");
            }
            if (catsModel.Items.Any())
            {
                if (ViewBag.Seller != null)
                {
                    var viewPath = string.Format("~/views/sellerarea/{0}/categoriescatalog.cshtml",
                        (ViewBag.Seller as Seller).EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default)
                        .ToString());
                    return View(viewPath, catsModel);
                }
                return View("CategoriesCatalog", catsModel);
            }

            var catalog = SellerService.GetSellerProductsCatalog(cachedCats, ViewBag.SellerUrl, categoryUrl, options);
            if (ViewBag.Seller != null)
            {
                var viewPath = string.Format("~/views/sellerarea/{0}/productscatalog.cshtml",
                    (ViewBag.Seller as Seller).EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default)
                    .ToString());
                return View(viewPath, catalog);
            }
            return View("ProductsCatalog", catalog);
        }

        public ActionResult GetProducts(string categoryId, string sellerId, string options, int page, SellerEcommerceTemplate? layout)
        {
            if (options.Contains("page"))
            {
                options = Regex.Replace(options, "page=\\d+", "page=" + page);
            }
            else
            {
                options += "page=" + page;
            }
            var products = SellerService.GetSellerCatalogProducts(sellerId, categoryId, options, ListConstants.DefaultTakePerPage, false).Products;
            var templateName = "_ProductPartial";
            if (layout.HasValue)
            {
                templateName = string.Format("~/Views/SellerArea/{0}/_ProductPartial.cshtml", layout.ToString());
            }
            var productsHtml = string.Join("", products.Take(ListConstants.DefaultTakePerPage).Select(entry => ControllerContext.RenderPartialToString(templateName, entry)));
            return Json(new { number = products.Count, products = productsHtml }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSellers(string options, int page)
        {
            var sellers = SellerService.GetSellersCatalog(options, page).Items;
            var sellersHtml = string.Join("", sellers.Select(entry => ControllerContext.RenderPartialToString("_SellerPartial", entry)));
            return Json(new { number = sellers.Count, sellers = sellersHtml }, JsonRequestBehavior.AllowGet);
        }
    }
}