using System;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.AdminRoleName + ", " + DomainConstants.SellerRoleName + ", " + DomainConstants.SellerModeratorRoleName)]
    public class NotificationsController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var channels = db.NotificationChannels.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId);
            return View(channels);
        }

        public ActionResult Create(NotificationChannelType channelType)
        {
            return PartialView("Create", channelType);
        }

        public ActionResult CreateOrUpdate(NotificationChannel model)
        {
            if (model.Address != null)
            {
                model.SellerId = Seller.CurrentAuthorizedSellerId;
                model.Id = Guid.NewGuid().ToString();
                db.NotificationChannels.Add(model);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Канал сповіщення збереженно";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Невірне значення";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(string id)
        {
            var channel = db.NotificationChannels.Find(id);
            db.NotificationChannels.Remove(channel);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Канал сповіщення видалено";
            return RedirectToAction("Index");
        }
    }
}