using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Web.Filters;

namespace Benefit.Web.Areas.Cabinet.Controllers
{
    [CabinetFilter]
    public class BenefitFamilyController : BasePartnerController
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            var userId = (ViewBag.User as ApplicationUser).Id;
            var user = db.Users.Include(entry => entry.BenefitCards)
                .FirstOrDefault(entry => entry.Id == userId);
            if (user.NFCCardNumber == null) RedirectToAction("Index", "Panel");
            return View(user);
        }

        public ActionResult CreateOrUpdate(string id)
        {
            var card = db.BenefitCards.Find(id) ?? new BenefitCard();
            return PartialView(card);
        }

        [HttpPost]
        public ActionResult CreateOrUpdate(string holderName, string cardNumber, string originalCardNumber)
        {
            var card = db.BenefitCards.Find(cardNumber);
            if (card != null)
            {
                if (card.UserId == null && db.Users.FirstOrDefault(entry => entry.CardNumber == cardNumber) == null)
                {
                    card.UserId = ViewBag.User.Id;
                    card.HolderName = holderName;
                    db.Entry(card).State = EntityState.Modified;
                    if (!string.IsNullOrEmpty(originalCardNumber))
                    {
                        Delete(originalCardNumber);
                    }
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Учасника успішно збережено";
                    return RedirectToAction("Index");
                }
            }
            TempData["ErrorMessage"] = "Неможливо зберегти цю картку";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(string id)
        {
            var card = db.BenefitCards.Find(id);
            card.UserId = null;
            card.HolderName = null;
            db.Entry(card).State = EntityState.Modified;
            db.SaveChanges();
            TempData["SuccessMessage"] = "Учасника успішно видалено";
            return RedirectToAction("Index");
        }
    }
}