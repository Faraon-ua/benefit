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
            /*
                        routes.MapRoute(
                name: "OneLevelNested",
                routeTemplate: "api/customers/{customerId}/orders/{orderId}",
                constraints: new { httpMethod = new HttpMethodConstraint(new string[] { "GET" }) },
                defaults: new { controller = "Customers", action = "GetOrders", orderId = RouteParameter.Optional, }
            );*/
            routes.MapRoute(
               name: RouteConstants.SellerCatalogRouteName,
               url: RouteConstants.SellersRoutePrefix + "/{sellerUrl}/" + RouteConstants.CategoriesRoutePrefix + "/{categoryUrl}",
               defaults: new { controller = "Postachalnyk", action = "Catalog", id = UrlParameter.Optional }
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
