using System.Web.Mvc;
using Benefit.DataTransfer.ViewModels;
using Benefit.Services;

namespace Benefit.Web.Controllers
{
    public class EmailController : Controller
    {
        EmailService EmailService = new EmailService();

        [HttpGet]
        public ActionResult SellerApplication()
        {
            return PartialView("_SellerApplication");
        }

        [HttpPost]
        public ActionResult SellerApplication(SellerApplicationViewModel sellerApplication)
        {
            EmailService.SendSellerApplication(sellerApplication);
            return Json(true);
        }

        [HttpPost]
        public ActionResult SendFacebookAccoutJoinRequest(string facebookAccount)
        {
            EmailService.SendEmail(EmailService.BenefitInfoEmail, facebookAccount,
                "Запит на підключення фейсбук сповіщень");
            return new HttpStatusCodeResult(200);
        }
	}
}