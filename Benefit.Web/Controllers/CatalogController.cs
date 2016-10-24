using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Services.Domain;

namespace Benefit.Web.Controllers
{
    public class CatalogController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index(string id)
        {
            var categoriesService = new CategoriesService();
            var sellers = categoriesService.GetCategorySellers(id);
            if (sellers == null) return HttpNotFound();
            return View(sellers);
        }
	}
}