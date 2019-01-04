using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels.Base;
using Benefit.DataTransfer.ViewModels.NavigationEntities;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Filters;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Benefit.Web.Controllers
{
    public class SellerController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private SellerService SellerService = new SellerService();

        [FetchCategories(Order = 1)]
        public ActionResult Index(string id, string category = null, string options = null)
        {
            var viewModel = new NavigationEntitiesViewModel<Product>();
            var seller = db.Sellers
                .Include(entry => entry.SellerCategories)
                .FirstOrDefault(entry => entry.UrlName == id);
            if (seller == null)
            {
                throw new HttpException(404, "Not found");
            }
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
                    .Take(ListConstants.DefaultTakePerPage * 2).ToList();
                viewModel.Items.AddRange(db.Products
                    .Include(entry => entry.Category)
                    .Include(entry => entry.Currency)
                    .Include(entry => entry.Images)
                    .Include(entry => entry.Seller.ShippingMethods.Select(sh => sh.Region))
                    .Where(entry =>
                        (!entry.Category.IsSellerCategory || (entry.Category.IsSellerCategory && entry.Category.MappedParentCategoryId != null)) &&
                        productIds.Contains(entry.Id) &&
                        (entry.AvailabilityState == ProductAvailabilityState.AlwaysAvailable ||
                         entry.AvailabilityState == ProductAvailabilityState.Available) && entry.Images.Any())
                    .ToList().OrderBy(entry => productIds.IndexOf(entry.Id))
                    .Take(ListConstants.DefaultTakePerPage + 1));
                if (productIds.Count < ListConstants.DefaultTakePerPage)
                {
                    viewModel.Items.AddRange(
                        db.Products
                            .Include(entry => entry.Currency)
                            .Include(entry => entry.Images)
                            .Where(entry => entry.SellerId == seller.Id)
                            .OrderBy(entry => entry.AvailabilityState)
                            .ThenByDescending(entry => entry.Images.Any())
                            .Take(ListConstants.DefaultTakePerPage + 1).ToList());
                }
                viewModel.Items.ForEach(entry =>
                {
                    if (entry.Currency != null)
                    {
                        entry.Price = entry.Price * entry.Currency.Rate;
                    }
                });
            }
            else
            {
                var selectedCat = categories.FindByUrlIdRecursively(category, null);
                if (selectedCat == null)
                {
                    throw new HttpException(404, "Not Found");
                }

                viewModel.Category = selectedCat;
                viewModel = SellerService.GetSellerProductsCatalog(categories, seller.UrlName, category, options);
            }

            viewModel.Breadcrumbs = null;
            viewModel.Seller = seller;
            return View("~/Views/Catalog/ProductsCatalog.cshtml", viewModel);
        }

        [FetchCategories(Order = 1)]
        public ActionResult Reviews(string id)
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