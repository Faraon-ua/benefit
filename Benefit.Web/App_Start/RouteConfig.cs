using System.Web.Mvc;
using System.Web.Routing;
using Benefit.Common.Constants;

namespace Benefit.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: RouteConstants.SellersRouteName,
               url: RouteConstants.SellersRoutePrefix + "/{id}/{action}",
               defaults: new { controller = "Postachalnyk", action = "Info", id = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: RouteConstants.CategoriesRouteName,
                url: RouteConstants.CategoriesRoutePrefix + "/{id}",
                defaults: new { controller = "Catalog", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
