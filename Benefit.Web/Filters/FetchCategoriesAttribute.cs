using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using WebGrease.Css.Extensions;

namespace Benefit.Web.Filters
{
    public class FetchCategoriesAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpRuntime.Cache["Categories"] != null)
            {
                filterContext.Controller.ViewBag.Categories = HttpRuntime.Cache["Categories"];
            }
            else
            {
                using (var db = new ApplicationDbContext())
                {
                    var categories = db.Categories
                        .Include(entry => entry.Products)
                        .Include(entry => entry.ChildCategories.Select(cat => cat.ChildCategories.Select(ch=>ch.Products)))
                        .Where(
                            entry =>
                                entry.ParentCategoryId == null && entry.IsActive && !entry.IsSellerCategory)
                        .OrderBy(entry => entry.Order)
                        .ToList();
                    categories.ForEach(entry =>
                    {
                        entry.ChildCategories.ForEach(child =>
                        {
                            child.ChildCategories = child.ChildCategories.Where(cat => cat.IsActive && cat.Products.Any()).ToList();
                        });
                        entry.ChildCategories = entry.ChildCategories.Where(cat => cat.IsActive && cat.ChildCategories.Any()).ToList();
                    });
                    categories = categories.Where(entry => entry.ChildCategories.Any() || entry.Products.Any()).ToList();
                    filterContext.Controller.ViewBag.Categories = categories;
                    HttpRuntime.Cache.Insert("Categories", categories, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(6));
                }
            }
        }
    }
}