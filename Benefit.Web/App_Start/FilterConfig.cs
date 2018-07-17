using System.Web.Mvc;
using Benefit.Web.Filters;

namespace Benefit.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new PasswordFilterAttribute());
            filters.Add(new SubdomainFilterAttribute());
            filters.Add(new HandleErrorAttribute());
            filters.Add(new UserFilterAttribute());
        }
    }
}
