﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Controllers
{
    public class TovarController : BaseController
    {
        //
        // GET: /Tovar/
        private ProductsService ProductsService { get; set; }
        private SellerService SellerService { get; set; }

        public TovarController()
        {
            ProductsService = new ProductsService();
            SellerService = new SellerService();
        }

        [FetchSeller]
        [FetchCategories]
        public ActionResult Index(string productUrl)
        {
            var cats = ViewBag.Categories as List<Category>;
            var productResult = ProductsService.GetProductDetails(cats, productUrl, User.Identity.GetUserId());
            var viewedProducts = Session[DomainConstants.ViewedProductsSessionKey] as List<Product> ??
                                 new List<Product>();
            viewedProducts.Insert(0, productResult.Product);
            viewedProducts = viewedProducts.Take(10).ToList();
            Session[DomainConstants.ViewedProductsSessionKey] = viewedProducts;
            productResult.ViewedProducts = viewedProducts;
            if (productResult == null) throw new HttpException(404, "Not found");
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                return View("~/views/sellerarea/product.cshtml", productResult);
            }
            return View(productResult);
        }

        public ActionResult GetProductOptions(string productId)
        {
            Product product;
            using (var db = new ApplicationDbContext())
            {
                product = db.Products.Find(productId);
            }
            if (product == null) return HttpNotFound();

            var result = new ProductDetailsViewModel()
            {
                Product = product,
                ProductOptions = ProductsService.GetProductOptions(product.Id),
            };
            if (result.ProductOptions.Any())
            {
                return PartialView("_ProductOptions", result);
            }
            return Content(string.Empty);
        }

        [HttpPost]
        public ActionResult AddReview(Review review)
        {
            review.Id = Guid.NewGuid().ToString();
            review.UserFullName = HttpUtility.UrlDecode(Request.Cookies[RouteConstants.FullNameCookieName].Value);
            review.Stamp = DateTime.UtcNow;
            if (review.Rating == null || review.Rating == default(int))
            {
                ModelState.AddModelError("Rating", "Рейтинг не вказано");
            }

            if (ModelState.IsValid)
            {
                using (var db = new ApplicationDbContext())
                {
                    db.Reviews.Add(review);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Ваш відгук з'явиться після модерації";
                }
            }
            else
            {
                return Json(new
                {
                    error = "Відгук невірно оформлений<br/>" + ModelState.ModelStateErrors()
                });
            }
            return new HttpStatusCodeResult(200);
        }

        [HttpGet]
        public ActionResult GetReviewCommentForm(string id)
        {
            return PartialView("_ReviewCommentPartial", id);
        }

        [HttpPost]
        public ActionResult AddReviewComment(Review review)
        {
            review.Id = Guid.NewGuid().ToString();
            review.UserFullName = User.IsInRole(DomainConstants.AdminRoleName) ? "Benefit Company" : HttpUtility.UrlDecode(Request.Cookies[RouteConstants.FullNameCookieName].Value);
            review.Stamp = DateTime.UtcNow;
            if (ModelState.IsValid)
            {
                using (var db = new ApplicationDbContext())
                {
                    db.Reviews.Add(review);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Ваш коментар з'явиться після модерації";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Коментар невірно оформлений<br/>" + ModelState.ModelStateErrors();
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }
    }
}