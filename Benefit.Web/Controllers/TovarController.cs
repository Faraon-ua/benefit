using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
        public ActionResult Index(string categoryUrl, string productUrl)
        {
            var product = ProductsService.GetProduct(productUrl);
            return View(product);
        }
        public ActionResult Index(string sellerUrl, string categoryUrl, string productUrl)
        {
            return View();
        }
	}
}