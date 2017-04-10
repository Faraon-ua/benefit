using System.Web.Mvc;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;

namespace Benefit.Web.Controllers
{
    [CustomKeyAuth]
    public class ScheduleController : Controller
    {
        public ActionResult GenerateSiteMap()
        {
            var siteMapHelper = new SiteMapHelper();
            var count = siteMapHelper.Generate(Url);
            TempData["SuccessMessage"] = "Файл sitemap.xml згенеровано. Кількість лінків: " + count;
            return Content(count.ToString());
        }
	}
}