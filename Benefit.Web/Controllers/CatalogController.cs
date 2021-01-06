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
using Benefit.DataTransfer.ViewModels.Base;
using Benefit.Domain.DataAccess;
using System.Configuration;
using Microsoft.AspNet.Identity;
using System.Data.SqlClient;
using System.Data;

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

        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        [FetchLastNews(Order = 2)]
        public ActionResult Index(string categoryUrl, string options)
        {
            var categoriesDbContext = new CategoiesDBContext();
            var category = categoriesDbContext.Get(categoryUrl);
            var seller = ViewBag.Seller as Seller;
            var cachedCats = ViewBag.Categories as List<CategoryVM>;

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
            if (category == null || (category != null && category.HasChildCategories))
            {
                CategoriesViewModel catsModel = CategoriesService.GetCategoriesCatalog(cachedCats, categoryUrl, ViewBag.SellerUrl);
                if (catsModel == null)
                {
                    throw new HttpException(404, "Not found");
                }
                if (catsModel.Items.Any())
                {
                    if (seller != null)
                    {
                        var viewPath = string.Format("~/views/sellerarea/{0}/categoriescatalog.cshtml",
                            seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default)
                            .ToString());
                        return View(viewPath, catsModel);
                    }
                    return View("CategoriesCatalog", catsModel);
                }
            }
            if (options != null && options.Contains("page=1;"))
            {
                Response.StatusCode = 301;
                var location =
                    Request.Url.AbsoluteUri.Replace("page=1;", string.Empty).Replace("page=1", string.Empty);
                Response.AddHeader("Location", location);
                Response.End();
            }
            var catalogService = new CatalogService();
            var model = catalogService.GetSellerProductsCatalog(seller == null ? null : seller.Id, category == null ? null : category.Id, User.Identity.GetUserId(), options);
            model.Category = category.MapToVM();
            model.Breadcrumbs = new BreadCrumbsViewModel()
            {
                Seller = seller,
                Categories = catalogService.GetBreadcrumbs(cachedCats, model.Category == null ? null : model.Category.Id)
            };
            if (seller != null)
            {
                if (!seller.HasEcommerce) return Content("Seller is not active or has no eCommerce");
                var viewPath = string.Format("~/views/sellerarea/{0}/productscatalog.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default)
                    .ToString());
                return View(viewPath, model);
            }
            return View("ProductsCatalog", model);
        }

        [FetchSeller(Order = 0)]
        public ActionResult GetProducts(string categoryId, string sellerId, string options, int page, SellerEcommerceTemplate? layout, bool isFilterRequest = false)
        {
            List<Product> products = null;
            var templateName = "_ProductPartial";
            if (options.Contains("page"))
            {
                options = Regex.Replace(options, "page=\\d+", "page=" + page);
            }
            else
            {
                options += "page=" + page;
            }
            var seller = ViewBag.Seller as Seller;
            var catalogService = new CatalogService();
            var catalog = catalogService.GetSellerProductsCatalog(seller == null ? null : seller.Id, categoryId, User.Identity.GetUserId(), options, false);
            products = catalog.Items;
            if (isFilterRequest)
            {
                templateName = seller == null ? "~/Views/Catalog/_ProductsListPartial.cshtml" :
                    string.Format("~/Views/SellerArea/{0}/_ProductListPartial.cshtml", layout.ToString());
                var html = ControllerContext.RenderPartialToString(templateName, catalog);
                return Json(new { number = catalog.PagesCount, products = html }, JsonRequestBehavior.AllowGet);
            }
            if (layout.HasValue)
            {
                templateName = seller == null ? "~/Views/Catalog/_ProductPartial.cshtml"
                    : string.Format("~/Views/SellerArea/{0}/_ProductPartial.cshtml", layout.ToString());
            }
            var productsHtml = string.Join("", products.Take(ListConstants.DefaultTakePerPage).Select(entry => ControllerContext.RenderPartialToString(templateName, entry)));
            return Json(new { number = products.Count, products = productsHtml }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSellers(string options)
        {
            var sellers = SellerService.GetSellersCatalog(options).Items;
            var sellersHtml = string.Join("", sellers.Take(ListConstants.DefaultTakePerPage).Select(entry => ControllerContext.RenderPartialToString("_SellerPartial", entry)));
            return Json(new { number = sellers.Count, products = sellersHtml }, JsonRequestBehavior.AllowGet);
        }
    }
}