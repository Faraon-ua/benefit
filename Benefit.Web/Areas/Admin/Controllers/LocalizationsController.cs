using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Web.Areas.Admin.Controllers.Base;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class LocalizationsController : AdminController
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index(string sellerId)
        {
            var products = db.Products.Where(entry => entry.SellerId == sellerId).OrderBy(entry=>entry.SKU).Skip(0).Take(50).ToList();
            var localizations = db.Localizations.Where(entry => products.Select(pr => pr.Id).Contains(entry.ResourceId) && entry.ResourceType == typeof(Product).ToString()).ToList();
            foreach (var product in products)
            {
                product.Localizations = localizations.Where(entry => entry.ResourceId == product.Id).ToList();
            }
            return View(products);
        }
	}
}