using System.Web.Mvc;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.Models;
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
        public ActionResult SendFacebookAccoutJoinRequest(string facebookAccount)
        {
            EmailService.SendEmail(EmailService.BenefitInfoEmail, string.Format("Постачальник: {0}, фейсбук: {1}", Seller.CurrentAuthorizedSellerName, facebookAccount),
                "Запит на підключення фейсбук сповіщень");
            return new HttpStatusCodeResult(200);
        }
	}
}