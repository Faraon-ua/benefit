using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;

namespace Benefit.Web.Controllers
{
    public class SellerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        { //var seller = db.Sellers
            //    .Include(entry => entry.SellerCategories.Select(sc => sc.Category))
            //    .FirstOrDefault(entry => entry.UrlName == id);
            //if (seller == null) return new HttpNotFoundResult();
            //var categories = seller.SellerCategories.Select(entry => entry.Category).ToList().SortByHierarchy().ToList();
            //var products = db.pro
            return View();
        }
    }
}