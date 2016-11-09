using System.Collections.Generic;
using System.Web.Mvc;
using Benefit.DataTransfer.ViewModels;
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
            var product = ProductsService.GetProductWithProductOptions(productUrl);
            if (product == null) return HttpNotFound();
            var categoriesService = new CategoriesService();

            var result = new ProductDetailsViewModel()
            {
                Product = product,
                CategoryUrl = categoryUrl,
                Breadcrumbs = new BreadCrumbsViewModel()
                {
                    Categories = categoriesService.GetBreadcrumbs(urlName: categoryUrl),
                    Product = product
                }
            };
            return View(result);
        }
      /*  public ActionResult Index(string sellerUrl, string categoryUrl)
        {
            return View();
        }*/
	}
}