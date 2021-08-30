﻿using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.DataTransfer.ViewModels.Base;
using Benefit.DataTransfer.ViewModels.NavigationEntities;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Filters;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Benefit.Web.Controllers
{
    public class SellerController : BaseController
    {
        private SellerService SellerService = new SellerService();

        [FetchCategories(Order = 1)]
        public ActionResult Index(string id, string category = null, string options = null)
        {
            var catalogService = new CatalogService();
            using (var db = new ApplicationDbContext())
            {
                var viewModel = new NavigationEntitiesViewModel<Product>();
                var seller = db.Sellers
                    .Include(entry => entry.InfoPages)
                    .Include(entry => entry.Reviews)
                    .Include(entry => entry.Images)
                    .Include(entry => entry.SellerCategories)
                    .FirstOrDefault(entry => entry.UrlName == id);
                if (seller == null)
                {
                    throw new HttpException(404, "Not found");
                }
                ViewBag.Seller = seller;
                ViewBag.IsSellerPage = true;
                var sellerCats = SellerService.GetAllSellerCategories(seller.UrlName);
                var categories = sellerCats.Where(entry => entry.ParentCategoryId == null).ToList().MapToVM();
                ViewBag.SellerCategories = categories;
                if (category == "golovna")
                {
                    viewModel = catalogService.GetSellerProductsCatalog(seller.Id, null, User.Identity.GetUserId(), options, true, true);
                    if (viewModel.Items.Count < ListConstants.DefaultTakePerPage)
                    {
                        viewModel.Items.AddRange(
                            db.Products
                                .Include(entry => entry.Currency)
                                .Include(entry => entry.Images)
                                .Include(entry => entry.Category.SellerCategories)
                                .Include(entry => entry.Category.MappedParentCategory.SellerCategories)
                                .Include(entry => entry.Seller.ShippingMethods.Select(sh => sh.Region))
                                .Where(entry => entry.SellerId == id && entry.IsActive && entry.Category.IsActive &&
                                                (!entry.Category.IsSellerCategory || entry.Category.IsSellerCategory &&
                                                 entry.Category.MappedParentCategory != null))
                                .OrderBy(entry => entry.AvailabilityState)
                                .ThenByDescending(entry => entry.Images.Any())
                                .Take(ListConstants.DefaultTakePerPage + 1).ToList());
                        viewModel.Items.ForEach(entry =>
                        {
                            var produCat = entry.Category.IsSellerCategory ? entry.Category.MappedParentCategory : entry.Category;
                            var sellerCategory = produCat.SellerCategories.FirstOrDefault(sc => sc.CategoryId == produCat.Id && sc.SellerId == entry.SellerId);
                            if (sellerCategory != null)
                            {
                                if (sellerCategory.CustomMargin.HasValue)
                                {
                                    if (entry.OldPrice.HasValue)
                                    {
                                        entry.OldPrice += entry.OldPrice * sellerCategory.CustomMargin.Value / 100;
                                    }
                                    entry.Price += entry.Price * sellerCategory.CustomMargin.Value / 100;
                                }
                            }
                            if (entry.Currency != null)
                            {
                                if (entry.OldPrice.HasValue)
                                {
                                    entry.OldPrice = (double)(entry.OldPrice * entry.Currency.Rate);
                                }
                                entry.Price = (double)(entry.Price * entry.Currency.Rate);
                            }
                        });
                    }
                    viewModel.Category = new CategoryVM { UrlName = "golovna" };
                    ViewBag.FetchFeatured = true;
                }
                else
                {
                    var selectedCat = categories.FindByUrlIdRecursively(category, null);
                    if (selectedCat == null)
                    {
                        throw new HttpException(404, "Not Found");
                    }
                    viewModel = catalogService.GetSellerProductsCatalog(seller == null ? null : seller.Id, selectedCat.Id, User.Identity.GetUserId(), options);
                    viewModel.Category = selectedCat;
                    viewModel.Breadcrumbs = new BreadCrumbsViewModel()
                    {
                        Seller = seller,
                        Categories = catalogService.GetBreadcrumbs(categories, selectedCat == null ? null : selectedCat.Id)
                    };
                    //viewModel.Category = selectedCat;
                    //viewModel = SellerService.GetSellerProductsCatalog(categories, seller.UrlName, category, options);
                }
                viewModel.Breadcrumbs = null;
                viewModel.Seller = seller;
                return View("~/Views/Catalog/ProductsCatalog.cshtml", viewModel);
            }
        }

        [FetchCategories(Order = 1)]
        public ActionResult Reviews(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var seller = db.Sellers.Include(entry => entry.Reviews).FirstOrDefault(entry => entry.UrlName == id);
                if (seller == null)
                {
                    throw new HttpException(404, "Not Found");
                }
                return View(seller);
            }
        }
    }
}