using System;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;

namespace Benefit.Web.Filters
{
    public class FetchLastNewsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpRuntime.Cache["LastNews"] != null)
            {
                filterContext.Controller.ViewBag.LastNews = HttpRuntime.Cache["LastNews"];
            }
            else
            {
                using (var db = new ApplicationDbContext())
                {
                    var lastNews = db.InfoPages.AsNoTracking().Where(entry => entry.IsNews && entry.IsActive).OrderByDescending(entry => entry.CreatedOn).Take(3).ToList();
                    filterContext.Controller.ViewBag.LastNews = lastNews;
                    HttpRuntime.Cache.Insert("LastNews", lastNews, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(6));
                }
            }
        }
    }
}