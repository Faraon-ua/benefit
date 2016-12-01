using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;

namespace Benefit.Web.Controllers
{
    public class TovarController : Controller
    {
        //
        // GET: /Tovar/
        ProductsService ProductsService { get; set; }

        public TovarController()
        {
            ProductsService = new ProductsService();
        }
        public ActionResult Index(string productUrl, string categoryUrl, string sellerUrl)
        {
            var product = ProductsService.GetProduct(productUrl);
            if (product == null) return HttpNotFound();
            var categoriesService = new CategoriesService();

            var result = new ProductDetailsViewModel()
            {
                Product = product,
                CategoryUrl = categoryUrl,
                ProductOptions = ProductsService.GetProductOptions(product.Id),
                Breadcrumbs = new BreadCrumbsViewModel()
                {
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
    }
}