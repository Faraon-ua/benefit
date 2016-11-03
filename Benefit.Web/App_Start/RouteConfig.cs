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
               name: RouteConstants.SellerCatalogRouteName,
               url: RouteConstants.SellersRoutePrefix + "/{sellerUrl}/" + RouteConstants.CategoriesRoutePrefix + "/{categoryUrl}",
               defaults: new { controller = "Postachalnyk", action = "Catalog", id = UrlParameter.Optional }
           ); 
            
            routes.MapRoute(
               name: RouteConstants.ProductRouteName,
               url: RouteConstants.CategoriesRoutePrefix + "/{categoryUrl}/" + RouteConstants.ProductRoutePrefix + "/{productUrl}",
               defaults: new { controller = "Tovar", action = "Index", id = UrlParameter.Optional }
           );
            
            routes.MapRoute(
               name: RouteConstants.ProductRouteName + "WithSeller",
               url: RouteConstants.SellersRoutePrefix + "/{sellerUrl}/" + RouteConstants.CategoriesRoutePrefix + "/{categoryUrl}/" + RouteConstants.ProductRoutePrefix + "/{productUrl}",
               defaults: new { controller = "Tovar", action = "Index", id = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: RouteConstants.CategoriesRouteName + "GetProducts",
                url: RouteConstants.CategoriesRoutePrefix + "/GetProducts/{id}",
                defaults: new { controller = "Catalog", action = "GetProducts", id = UrlParameter.Optional }
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
