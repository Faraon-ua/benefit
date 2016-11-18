using System.Web.Mvc;
using Benefit.DataTransfer.ViewModels;
using Benefit.Services;

namespace Benefit.Web.Controllers
{
    public class EmailController : Controller
    {
        [HttpGet]
        public ActionResult SellerApplication()
        {
            return PartialView("_SellerApplication");
        }

        [HttpPost]
        public ActionResult SellerApplication(SellerApplicationViewModel sellerApplication)
        {
            var emailService = new EmailService();
            emailService.SendSellerApplication(sellerApplication);
            return Json(true);
        }
	}
}