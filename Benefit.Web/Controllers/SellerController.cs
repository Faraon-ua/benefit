using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using System.Data.Entity;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels.Base;
using Benefit.DataTransfer.ViewModels.NavigationEntities;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using Benefit.Web.Filters;

namespace Benefit.Web.Controllers
{
    public class SellerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private SellerService SellerService = new SellerService();


        [FetchCategories]
        public ActionResult Index(string id, string category = null, string options = null)
        {
            var viewModel = new NavigationEntitiesViewModel<Product>();
            var seller = db.Sellers
                .Include(entry => entry.SellerCategories)
                .FirstOrDefault(entry => entry.UrlName == id);
            if (seller == null) return new HttpNotFoundResult();
            ViewBag.Seller = seller;
            var sellerCats = SellerService.GetAllSellerCategories(seller.UrlName);
            var categories = sellerCats.Where(entry => entry.ParentCategoryId == null).ToList();
            ViewBag.SellerCategories = categories;
            if (string.IsNullOrEmpty(category))
            {
                viewModel = new ProductsViewModel();
                var productIds = db.Orders
                    .Where(entry => entry.SellerId == seller.Id)
                    .SelectMany(entry => entry.OrderProducts)
                    .Select(entry => entry.ProductId)
                    .GroupBy(entry => entry)
                    .OrderByDescending(s => s.Count())
                    .Select(entry => entry.Key)
                    .Take(ListConstants.DefaultTakePerPage).ToList();
                if (productIds.Any())
                {
                    viewModel.Items = db.Products
                        .Where(entry => productIds.Contains(entry.Id)).ToList();
                }
                else
                {
                    viewModel.Items = db.Products.Where(entry => entry.SellerId == seller.Id).Take(ListConstants.DefaultTakePerPage).ToList();
                }
            }
            else
            {
                viewModel.Category = categories.FindByUrlIdRecursively(category, null);
                viewModel = SellerService.GetSellerProductsCatalog(categories, seller.UrlName, category, options);
            }

            viewModel.Breadcrumbs = null;
            viewModel.Seller = seller;
            return View("~/Views/Catalog/ProductsCatalog.cshtml", viewModel);
        }

        [FetchCategories]
        public ActionResult Reviews(string id)
        {
            var seller = db.Sellers.Include(entry => entry.Reviews).FirstOrDefault(entry => entry.UrlName == id);
            if(seller == null) throw  new HttpException(404, "Not Found");
            return View(seller);
        }
    }
}