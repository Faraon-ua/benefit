using System.Web.Mvc;

namespace Benefit.Web.Attributes
{
    public class CustomAuthorization : AuthorizeAttribute
    {
        public string Url { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.HttpContext.Response.Redirect(Url);
            }
            base.OnAuthorization(filterContext);
        }
    }
}