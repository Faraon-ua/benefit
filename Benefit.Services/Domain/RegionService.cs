using System.Configuration;
using System.Linq;
using System.Web;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;

namespace Benefit.Services.Domain
{
    public class RegionService
    {
        public static int GetRegionId()
        {
            var regionId = 400000;
            if (HttpContext.Current != null)
            {
                var regionIdCookie = HttpContext.Current.Request.Cookies[DomainConstants.RegionIdCookieKey];
                if (regionIdCookie != null)
                {
                    var regionIdStr = regionIdCookie.Value;
                    int.TryParse(regionIdStr, out regionId);
                    return regionId;
                }
            }
            return regionId;
        }

        public static string GetRegionName()
        {
            if (HttpContext.Current != null)
            {
                var regionNameCookie = HttpContext.Current.Request.Cookies[DomainConstants.RegionNameCookieKey];
                if (regionNameCookie != null)
                {
                    return regionNameCookie.Value;
                }
            }
            return null;
        }
    }
}
