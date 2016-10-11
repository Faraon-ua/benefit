using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Web.Controllers.Base;

namespace Benefit.Web.Controllers
{
    public class LayoutController : BaseController
    {
        //
        // GET: /Layout/
        CacheService CacheService { get; set; }
        ApplicationDbContext db = new ApplicationDbContext();

        public LayoutController()
        {
            CacheService = new CacheService();
        }

        public ActionResult GetCategoriesMenu(string parentId)
        {

            return PartialView("_CategoriesMenu,");
        }
	}
}