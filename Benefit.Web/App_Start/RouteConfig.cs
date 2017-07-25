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
               name: RouteConstants.ProductRouteWithSellerName,
               url:
                   RouteConstants.SellersRoutePrefix + "/{sellerUrl}/" + RouteConstants.CategoriesRoutePrefix +
                   "/{categoryUrl}/" + RouteConstants.ProductRoutePrefix + "/{productUrl}",
               defaults: new { controller = "Tovar", action = "Index", id = UrlParameter.Optional }
               );

            routes.MapRoute(
               name: "AllSellers",
               url: RouteConstants.SellersRoutePrefix + "/vsi/{options}",
               defaults: new { controller = "Postachalnyk", action = "Vsi", options = UrlParameter.Optional }
               );

            routes.MapRoute(
               name: "OldRegister",
               url: "partner/{id}",
               defaults: new { controller = "Account", action = "Register", id = UrlParameter.Optional });

            routes.MapRoute(
               name: "GetProductsRoute",
               url:
                   RouteConstants.CategoriesRoutePrefix + "/GetProducts",
               defaults: new { controller = "Catalog", action = "GetProducts" }
               );

            routes.MapRoute(
               name: RouteConstants.SellerCatalogRouteName,
               url:
                   RouteConstants.SellersRoutePrefix + "/{sellerUrl}/" + RouteConstants.CategoriesRoutePrefix +
                   "/{categoryUrl}/{options}",
               defaults: new { controller = "Postachalnyk", action = "Catalog", sellerUrl = UrlParameter.Optional, categoryUrl = UrlParameter.Optional, options = UrlParameter.Optional }
               );

            routes.MapRoute(
             name: RouteConstants.CatalogRouteName,
             url:
                 RouteConstants.CategoriesRoutePrefix + "/{categoryUrl}/{options}",
             defaults: new { controller = "Catalog", action = "Index", categoryUrl = UrlParameter.Optional, options = UrlParameter.Optional }
             );

            routes.MapRoute(
                name: RouteConstants.SellersRouteName,
                url: RouteConstants.SellersRoutePrefix + "/{id}/{action}",
                defaults: new { controller = "Postachalnyk", action = "Info", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: RouteConstants.ProductRouteName,
                url:
                    RouteConstants.CategoriesRoutePrefix + "/{categoryUrl}/" + RouteConstants.ProductRoutePrefix +
                    "/{productUrl}",
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
                name: "newsRoute",
                url: "pages/news",
                defaults: new { controller = "Pages", action = "News", id = UrlParameter.Optional }
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
