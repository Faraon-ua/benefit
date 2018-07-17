using System.Web.Mvc;
using Benefit.Web.Filters;

namespace Benefit.Web.Controllers.Base
{
    [Authorize(Roles = "Admin,SellerModerator")]
    public class BaseController : Controller
    {
    
	}
}