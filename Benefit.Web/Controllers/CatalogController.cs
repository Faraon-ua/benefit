﻿using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Services.Domain;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;
using Benefit.DataTransfer.ViewModels.NavigationEntities;

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

        [FetchSeller]
        [FetchCategories]
        [FetchLastNews]
        public ActionResult Index(string categoryUrl, string options)
        {
            if (categoryUrl == "postachalnuky")
            {
                var sellers = SellerService.GetSellersCatalog(options);
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

            CategoriesViewModel catsModel = CategoriesService.GetCategoriesCatalog(categoryUrl, ViewBag.SellerUrl);
            if (catsModel == null)
            {
                return HttpNotFound();
            }
            if (catsModel.Items.Any())
            {
                if (ViewBag.Seller != null)
                {
                    return View("~/views/sellerarea/categoriescatalog.cshtml", catsModel);
                }
                return View("CategoriesCatalog", catsModel);
            }

            var catalog = SellerService.GetSellerProductsCatalog(ViewBag.SellerUrl, categoryUrl, options);
            if (ViewBag.Seller != null)
            {
                return View("~/views/sellerarea/productscatalog.cshtml", catalog);
            }
            return View("ProductsCatalog", catalog);
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

        public ActionResult GetSellers(string options, int page)
        {
            var sellers = SellerService.GetSellersCatalog(options, page).Items;
            var sellersHtml = string.Join("", sellers.Select(entry => ControllerContext.RenderPartialToString("_SellerPartial", entry)));
            return Json(new { number = sellers.Count, sellers = sellersHtml }, JsonRequestBehavior.AllowGet);
        }
    }
}