using System.Web;

namespace Benefit.Web.Helpers
{
    public class RouteDataHelper
    {
        public static string ControllerName
        {
            get
            {
                return HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString().ToLower();
            }
        }

        public static string ActionName
        {
            get
            {
                return HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString().ToLower();
            }
        }
    }
}