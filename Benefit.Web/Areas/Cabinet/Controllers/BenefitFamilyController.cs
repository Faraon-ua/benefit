﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Web.Areas.Cabinet.Models;
using Benefit.Web.Filters;

namespace Benefit.Web.Areas.Cabinet.Controllers
{
    [CabinetFilter]
    public class BenefitFamilyController : BasePartnerController
    {
        public ActionResult Index()
        {
            using (var db = new ApplicationDbContext())
            {
                var userId = (ViewBag.User as ApplicationUser).Id;
                var user = db.Users.Include(entry => entry.BenefitCards)
                    .FirstOrDefault(entry => entry.Id == userId);
                if (user.NFCCardNumber == null) RedirectToAction("Index", "Panel");
                return View(user);
            }
        }

        public ActionResult CreateOrUpdate(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var card = db.BenefitCards.FirstOrDefault(entry => entry.Id == id) ?? new BenefitCard();
                return PartialView(card);
            }
        }

        [HttpPost]
        public ActionResult CreateOrUpdate(string holderName, string cardNumber)
        {
            using (var db = new ApplicationDbContext())
            {
                //todo: remove BC remove workaround
                cardNumber = cardNumber.Replace("BC", "");
                var card = db.BenefitCards.FirstOrDefault(entry => entry.Id == cardNumber);
                if (card != null)
                {
                    if (card.UserId == null && db.Users.FirstOrDefault(entry => entry.CardNumber == cardNumber) == null)
                    {
                        card.UserId = ViewBag.User.Id;
                        card.HolderName = holderName;
                        db.Entry(card).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Учасника успішно збережено";
                        return RedirectToAction("Index");
                    }
                }
                var unsuccessfulAttempts = Request.Cookies["unsuccessfulAttempts"] == null
                    ? 0
                    : int.Parse(Request.Cookies["unsuccessfulAttempts"].Value);
                unsuccessfulAttempts++;
                var unsuccessfulAttemptsCookie = new System.Web.HttpCookie("unsuccessfulAttempts")
                {
                    Value = unsuccessfulAttempts.ToString(),
                    Expires = DateTime.Now.AddMinutes(3)
                };
                Response.Cookies.Add(unsuccessfulAttemptsCookie);

                if (unsuccessfulAttempts >= 3)
                {
                    return RedirectToAction("contact_us", "Panel", new { subject = "Потрібна допомога в активації картки/брелка", body = cardNumber });
                }
                TempData["ErrorMessage"] = "Неможливо зберегти цю картку";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = DomainConstants.AdminRoleName)]
        public ActionResult Delete(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var card = db.BenefitCards.FirstOrDefault(entry => entry.Id == id);
                card.UserId = null;
                card.HolderName = null;
                db.Entry(card).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Учасника успішно видалено";
                return RedirectToAction("Index", new { userId = ViewBag.User.Id });
            }
        }

        public ActionResult RegisteredCards(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var registeredCards = new RegisteredCardsViewModel()
                {
                    Available =
                    db.BenefitCards.Where(
                        entry =>
                            entry.ReferalUserId == id && !db.BenefitCards.Select(bc => entry.UserId).Contains(id) &&
                            !db.Users.Select(us => us.NFCCardNumber).Contains(entry.NfcCode)).ToList(),
                    Registered =
                    db.BenefitCards.Where(
                        entry =>
                            entry.ReferalUserId == id && db.BenefitCards.Select(bc => entry.UserId).Contains(id) &&
                            db.Users.Select(us => us.NFCCardNumber).Contains(entry.NfcCode)).ToList()
                };
                ViewBag.UserId = id;
                return PartialView("_RegisteredCards", registeredCards);
            }
        }

        [ValidateAntiForgeryToken]
        [Authorize(Roles = DomainConstants.AdminRoleName)]
        public ActionResult AddRegisteredCard(string userId, string cardNumber)
        {
            using (var db = new ApplicationDbContext())
            {
                var card = db.BenefitCards.FirstOrDefault(entry => entry.Id == cardNumber);
                if (card == null) return HttpNotFound();
                if (card.ReferalUserId == null && !db.Users.Any(entry => entry.CardNumber == cardNumber))
                {
                    card.ReferalUserId = userId;
                    db.Entry(card).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    return Json(new { error = "Ця картка не може бути задіяна" });
                }
                return RedirectToAction("RegisteredCards", new { id = userId });
            }
        }

        [Authorize(Roles = DomainConstants.AdminRoleName)]
        public ActionResult RemoveRegisteredCard(string cardNumber)
        {
            using (var db = new ApplicationDbContext())
            {
                var card = db.BenefitCards.FirstOrDefault(entry => entry.Id == cardNumber);
                if (card != null)
                {
                    card.ReferalUserId = null;
                    db.Entry(card).State = EntityState.Modified;
                    db.SaveChanges();
                    return new HttpStatusCodeResult(200);
                }
                return HttpNotFound();
            }
        }
    }
}