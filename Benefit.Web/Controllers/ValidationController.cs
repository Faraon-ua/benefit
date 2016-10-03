using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using Benefit.Domain.DataAccess;

namespace Benefit.Web.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    public class ValidationController : Controller
    {
        public JsonResult IsCardUnique(string cardNumber)
        {
            using (var db = new ApplicationDbContext())
            {
                var result = !db.Users.Any(entry => entry.CardNumber == cardNumber);
                if (result)
                    return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }
    }
}