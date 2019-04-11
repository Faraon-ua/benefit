using System.Linq;
using System.Web.Mvc;
using Benefit.DataTransfer.JSON;
using Benefit.Domain.DataAccess;

namespace Benefit.Web.Controllers
{
    public class ProductRegionsController : Controller
    {
        public ActionResult GetSelectRegionsForm()
        {
            return PartialView("_SelectRegionPartial");
        }

        [HttpGet]
        public ActionResult SearchRegion(string query, int? minLevel = 4)
        {
            query = query.ToLower();
            using (var db = new ApplicationDbContext())
            {
                var regions =
                    db.Regions.Where(
                            entry =>
                                (entry.RegionLevel >= minLevel || entry.RegionLevel == 0) &&
                                (entry.Name_ru.ToLower().Contains(query) || entry.Name_ua.ToLower().Contains(query)))
                        .OrderBy(entry => entry.RegionLevel)
                        .Take(50)
                        .ToList();
                var result = new AutocompleteSearch
                {
                    query = query,
                    suggestions = regions.Select(entry => new ValueData()
                    {
                        value = entry.ExpandedName,
                        data = entry.Id.ToString()
                    }).ToArray()
                };
                return Json(result,
                    JsonRequestBehavior.AllowGet);
            }
        }
    }
}