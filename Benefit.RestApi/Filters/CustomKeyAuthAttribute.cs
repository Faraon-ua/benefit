using System.Net;
using System.Web.Mvc;
using Benefit.Services;

namespace Benefit.RestApi.Filters
{
    public class CustomKeyAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var key = filterContext.RequestContext.HttpContext.Request.QueryString["key"];
            if (key == null || key != SettingsService.ScheduleKey)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }
    }
}