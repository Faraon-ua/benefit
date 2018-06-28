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
                name: RouteConstants.SellerCatalogRouteName,
                url: RouteConstants.SellerCatalogRoutePrefix + "/{id}/{category}/{options}",
                defaults: new
                {
                    controller = "Seller",
                    action = "Index",
                    id = UrlParameter.Optional,
                    category  = UrlParameter.Optional,
                    options = UrlParameter.Optional
                }
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
                name: RouteConstants.SearchRouteName + "Autocomplete",
                url: RouteConstants.SearchRoutePrefix + "/searchwords",
                defaults: new
                {
                    controller = "Search",
                    action = "SearchWords",
                    options = UrlParameter.Optional
                }
            );


            routes.MapRoute(
                name: RouteConstants.SearchRouteName,
                url: RouteConstants.SearchRoutePrefix + "/{options}",
                defaults: new
                {
                    controller = "Search",
                    action = "Index",
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
