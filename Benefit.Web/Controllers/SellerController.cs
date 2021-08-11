using Benefit.Common.Constants;
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
                var sellerCats = SellerService.GetAllSellerCategories(seller.UrlName);
                var categories = sellerCats.Where(entry => entry.ParentCategoryId == null).ToList().MapToVM();
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
                        .Take(ListConstants.DefaultTakePerPage * 10).ToList();
                    viewModel.Items.AddRange(db.Products
                        .Include(entry => entry.Category.SellerCategories)
                        .Include(entry => entry.Category.MappedParentCategory.SellerCategories)
                        .Include(entry => entry.Currency)
                        .Include(entry => entry.Images)
                        .Include(entry => entry.Seller.ShippingMethods.Select(sh => sh.Region))
                        .Where(entry =>
                            (!entry.Category.IsSellerCategory || (entry.Category.IsSellerCategory && entry.Category.MappedParentCategoryId != null)) &&
                            productIds.Contains(entry.Id) &&
                            (entry.AvailabilityState == ProductAvailabilityState.AlwaysAvailable ||
                             entry.AvailabilityState == ProductAvailabilityState.Available)
                             && entry.IsActive
                             && entry.Seller.IsActive
                             && entry.Category.IsActive
                             && entry.Images.Any()));
                    if (productIds.Count < ListConstants.DefaultTakePerPage)
                    {
                        viewModel.Items.AddRange(
                            db.Products
                                .Include(entry => entry.Currency)
                                .Include(entry => entry.Images)
                                .Include(entry => entry.Category.SellerCategories)
                                .Include(entry => entry.Category.MappedParentCategory.SellerCategories)
                                .Include(entry => entry.Seller.ShippingMethods.Select(sh => sh.Region))
                                .Where(entry => entry.SellerId == seller.Id && entry.IsActive && entry.Category.IsActive &&
                                                (!entry.Category.IsSellerCategory || entry.Category.IsSellerCategory &&
                                                 entry.Category.MappedParentCategory != null))
                                .OrderBy(entry => entry.AvailabilityState)
                                .ThenByDescending(entry => entry.Images.Any())
                                .Take(ListConstants.DefaultTakePerPage + 1).ToList());
                    }
                    viewModel.Items = viewModel.Items.Distinct(new ProductComparer()).ToList();
                    viewModel.Items.ForEach(entry =>
                    {
                        if (entry.AvailabilityState == ProductAvailabilityState.Available && (entry.AvailableAmount == null || entry.AvailableAmount == 0)
                        || !entry.IsActive
                        || !entry.Seller.IsActive
                        || !entry.Category.IsActive)
                        {
                            entry.AvailabilityState = ProductAvailabilityState.NotInStock;
                        }
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
                    ((ProductsViewModel)viewModel).PagesCount = (viewModel.Items.Count - 1) / ListConstants.DefaultTakePerPage + 1;
                }
                else
                {
                    var selectedCat = categories.FindByUrlIdRecursively(category, null);
                    if (selectedCat == null)
                    {
                        throw new HttpException(404, "Not Found");
                    }
                    var catalogService = new CatalogService();
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