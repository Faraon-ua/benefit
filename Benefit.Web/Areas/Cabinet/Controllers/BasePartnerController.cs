using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Benefit.Web.Areas.Cabinet.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class BasePartnerController : Controller
    {
	}
}