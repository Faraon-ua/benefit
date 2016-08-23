using System.Web.Mvc;

namespace Benefit.Web.Areas.Admin.Controllers.Base
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
    }
}