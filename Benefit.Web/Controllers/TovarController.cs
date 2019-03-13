using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.DataTransfer.ViewModels.NavigationEntities;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

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

        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public ActionResult Index(string productUrl)
        {
            var cats = ViewBag.Categories as List<Category>;
            var productResult = ProductsService.GetProductDetails(cats, productUrl, User.Identity.GetUserId());
            if (productResult == null)
            {
                throw new HttpException(404, "Not found");
            }
            var viewedProducts = Session[DomainConstants.ViewedProductsSessionKey] as List<Product> ??
                                 new List<Product>();
            if (!viewedProducts.Contains(productResult.Product, new ProductComparer()))
            {
                viewedProducts.Insert(0, productResult.Product);
                viewedProducts = viewedProducts.Take(10).ToList();
                Session[DomainConstants.ViewedProductsSessionKey] = viewedProducts;
            }
            productResult.ViewedProducts = viewedProducts;
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                var viewName = seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default).ToString();
                return View(string.Format("~/views/sellerarea/{0}/product.cshtml", viewName), productResult);
            }
            return View(productResult);
        }

        [FetchSeller]
        public ActionResult GetViewedProducts()
        {
            var viewedProducts = Session[DomainConstants.ViewedProductsSessionKey] as List<Product>;
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                var viewName = seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default).ToString();
                return PartialView(string.Format("~/views/sellerarea/{0}/_ViewedProducts.cshtml", viewName), viewedProducts);
            }
            return PartialView("_ViewedProducts.cshtml", viewedProducts);

        }

        public ActionResult GetProductOptions(string productId)
        {
            Product product;
            using (var db = new ApplicationDbContext())
            {
                product = db.Products.Find(productId);
            }
            if (product == null)
            {
                return HttpNotFound();
            }

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

        public ActionResult GetProductVariants(string productId)
        {
            Product product;
            using (var db = new ApplicationDbContext())
            {
                product = db.Products.Include(entry=>entry.ProductOptions).FirstOrDefault(entry=>entry.Id == productId);
            }
            if (product == null)
            {
                return HttpNotFound();
            }

            product.ProductOptions = product.ProductOptions.Where(entry => entry.IsVariant).ToList();
            if (product.ProductOptions.Any())
            {
                return PartialView("_ProductVariants", product);
            }
            return Content(string.Empty);
        }

        [HttpPost]
        public ActionResult AddToFavorites(string productId, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                var favoritesCount = ProductsService.AddToFavorites(User.Identity.GetUserId(), productId);
                if (favoritesCount != null)
                {
                    return Json(new { message = "Товар додано до улюблених", favoritesCount.count, favoritesCount.sellercount, favoritesCount.sellerurl }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { message = "Товар вже додано до улюблених" }, JsonRequestBehavior.AllowGet);
            }

            return Json(
                new
                {
                    message = string.Format(
                        "<a href='{0}'>Авторизуйтесь</a> або <a href='{1}#reg'>зареєструйтесь</a>,<br> щоб додати товар до улюбленого",
                        Url.Action("login", "account", new { returnUrl }), Url.Action("login", "account", new { returnUrl }))
                }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RemoveFromFavorites(string productId)
        {
            var favoritesCount = ProductsService.RemoveFromFavorites(User.Identity.GetUserId(), productId);
            return Json(new { message = "Товар видалено із улюблених", favoritesCount.count, favoritesCount.sellercount, favoritesCount.sellerurl }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public ActionResult Favorites()
        {
            var userId = User.Identity.GetUserId();
            var products = ProductsService.GetFavorites(userId);
            var model = new ProductsViewModel
            {
                Category = new Category() { Name = "Улюблені товари", Title = "Улюблені товари", UrlName = "favorites" },
                Items = products,
                Breadcrumbs = new BreadCrumbsViewModel() { Page = new InfoPage() { Name = "Улюблені товари" } },
                IsFavorites = true
            };
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                model.Items = model.Items.Where(entry => entry.SellerId == seller.Id).ToList();
                var viewPath = string.Format("~/views/sellerarea/{0}/productscatalog.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default)
                    .ToString());
                return View(viewPath, model);
            }
            return View("~/Views/Catalog/ProductsCatalog.cshtml", model);
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