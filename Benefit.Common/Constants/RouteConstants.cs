using System.Collections.Generic;

namespace Benefit.Common.Constants
{
    public class RouteConstants
    {
        public const string AdminAreaName = "Admin";

        public const string SearchRouteName = "SearchRoute";
        public const string SearchRoutePrefix = "search";

        public const string CatalogRouteName = "CatalogRoute";
        public const string CatalogRoutePrefix = "catalog";

        public const string SellerCatalogRouteName = "SellerCatalogRoute";
        public const string SellerCatalogRoutePrefix = "seller";

        public const string ProductRouteName = "ProductRoute";
        public const string ProductRoutePrefix = "t";

        public const string ReferalUrlName = "referal";
        public const string ReferalCookieName = "ReferalNumber";
        public const string SelfReferalCookieName = "SelfReferalNumber";
        public const string FullNameCookieName = "FullName";
        public const string RegionNameCookieName = "regionName";
        public const string RegionIdCookieName = "regionId";

        public static Dictionary<string, string> Redirections = new Dictionary<string, string>
        {
            {"koshik24.benefit-company.com", "koshik24.com.ua"}
        };
    }
}
