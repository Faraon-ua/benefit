using System.Web.Mvc;

namespace Benefit.Web.Areas.Cabinet
{
    public class CabinetAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Cabinet";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Cabinet_default",
                "Cabinet/{controller}/{action}/{id}",
                new { controller = "Panel", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}