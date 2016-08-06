using System.Data.Entity;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using System.Linq;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        public ApplicationDbContext db = new ApplicationDbContext();
        //
        // GET: /Admin/Users/
        public ActionResult Index(string search)
        {
            var usersCount = db.Users.Count();
            return View(usersCount);
        }
        
        public ActionResult UsersSearch (string search)
        {
            search = search.ToLower();
            var users = db.Users.Where(entry => entry.FullName.ToLower().Contains(search) ||
                entry.Email.ToLower().Contains(search) ||
                entry.PhoneNumber.ToLower().Contains(search) ||
                entry.UserName.ToLower().Contains(search) ||
                entry.CardNumber.ToString().Contains(search));
            return PartialView("_UsersSearch", users);
        }
	}
}