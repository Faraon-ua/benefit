﻿using System.Collections.Generic;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;
using Benefit.Services.Domain;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Controllers
{
    public class PostachalnykController : Controller
    {
        SellerService SellerService = new SellerService();
        //
        // GET: /Postachalnyk/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Vsi()
        {
            var model = SellerService.GetAllSellers();
            model.Category = new Category();
            model.Category.Name = "Всі постачальники";
            model.Breadcrumbs = new BreadCrumbsViewModel()
            {
                Categories = new List<Category>() { new Category() { Name = "Всі постачальники" } }
            };
            return View("~/Views/Catalog/SellersCatalog.cshtml", model);
        }

        public ActionResult Info(string id)
        {
            var referrer = Request.UrlReferrer;
            string categoryUrlName = null;
            if (referrer != null && referrer.PathAndQuery.Contains(RouteConstants.CategoriesRoutePrefix))
            {
                var catalogIndexOf = referrer.PathAndQuery.IndexOf(RouteConstants.CategoriesRoutePrefix);
                var afterCatalogIndexOf = catalogIndexOf + RouteConstants.CategoriesRoutePrefix.Length + 1;
                if (afterCatalogIndexOf < referrer.PathAndQuery.Length)
                {
                    categoryUrlName = referrer.PathAndQuery.Substring(afterCatalogIndexOf);
                    var tovarIndexOf = categoryUrlName.IndexOf(RouteConstants.ProductRoutePrefix);
                    if (tovarIndexOf > 0)
                    {
                        categoryUrlName = categoryUrlName.Substring(0, tovarIndexOf - 1);
                    }
                }
            }
            var sellerVm = SellerService.GetSellerDetails(id, categoryUrlName, User.Identity.GetUserId());
            if (sellerVm.Seller == null) return HttpNotFound();
            return View(sellerVm);
        }

        [HttpGet]
        public ActionResult Catalog(string sellerUrl = null, string categoryUrl = null, string options = null, ProductSortOption? sort = null)
        {
            var model = SellerService.GetSellerCatalog(sellerUrl, categoryUrl, options);
            return View("../Catalog/ProductsCatalog", model);
        }

       /* [HttpGet]
        public ActionResult ProductsList(string sellerId = null, string categoryId = null, string options = null,
            ProductSortOption? sort = null)
        {
            var products = SellerService.GetSellerCatalogProducts(sellerId, categoryId, sort.GetValueOrDefault(ProductSortOption.Order));
            return PartialView("ProductsList", products);
        }*/
    }
}