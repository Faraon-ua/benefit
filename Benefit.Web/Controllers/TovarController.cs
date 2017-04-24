using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using Benefit.Web.Helpers;

namespace Benefit.Web.Controllers
{
    public class TovarController : Controller
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
        public ActionResult Index(string productUrl, string categoryUrl, string sellerUrl)
        {
            var seller = SellerService.GetSellerWithShippingMethods(sellerUrl);
            var product = ProductsService.GetProduct(productUrl);
            if (product == null) return HttpNotFound();
            product.Seller = seller;
            var categoriesService = new CategoriesService();

            var result = new ProductDetailsViewModel()
            {
                Product = product,
                CategoryUrl = categoryUrl,
                ProductOptions = ProductsService.GetProductOptions(product.Id),
                Breadcrumbs = new BreadCrumbsViewModel()
                {
                    Seller = seller,
                    Categories = categoriesService.GetBreadcrumbs(urlName: categoryUrl),
                    Product = product
                }
            };
            return View(result);
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
                TempData["ErrorMessage"] = "Відгук невірно оформлений<br/>" + ModelState.ModelStateErrors();
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }
    }
}