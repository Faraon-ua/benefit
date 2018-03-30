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
              name: RouteConstants.CatalogRouteName + "GetProducts",
              url: RouteConstants.CatalogRoutePrefix + "/getproducts",
              defaults: new { controller = "Catalog", action = "GetProducts" }
              );

            routes.MapRoute(
              name: RouteConstants.CatalogRouteName + "GetSellers",
              url: RouteConstants.CatalogRoutePrefix + "/getsellers",
              defaults: new { controller = "Catalog", action = "GetSellers" }
              );

            routes.MapRoute(
                name: RouteConstants.CatalogRouteName,
                url: RouteConstants.CatalogRoutePrefix + "/{categoryUrl}/{options}",
                defaults: new
                {
                    controller = "Catalog",
                    action = "Index",
                    categoryUrl = UrlParameter.Optional,
                    productUrl = UrlParameter.Optional,
                    options = UrlParameter.Optional
                }
            );

            routes.MapRoute(
                name: RouteConstants.ProductRouteName,
                url: RouteConstants.ProductRoutePrefix + "/{productUrl}",
                defaults: new
                {
                    controller = "Tovar",
                    action = "Index",
                    productUrl = UrlParameter.Optional
                }
            );

            routes.MapRoute(
                name: "pagesRoute",
                url: "pages/{id}",
                defaults: new { controller = "Pages", action = "Index", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                );
        }
    }
}
