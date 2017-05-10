using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;

namespace Benefit.Web.Areas.Admin.Controllers.Base
{
    [Authorize(Roles = DomainConstants.AdminRoleName)]
    public class CardsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Admin/Cards/
        public ActionResult Index(string CardNumber, int page = 0)
        {
            var benefitcards = db.BenefitCards.Include(b => b.User);
            if (!string.IsNullOrEmpty(CardNumber))
            {
                benefitcards = benefitcards.Where(entry => entry.Id == CardNumber);
            }
            else
            {
                benefitcards = benefitcards.OrderBy(entry=>entry.Id).Skip(page * ListConstants.DefaultTakePerPage).Take(ListConstants.DefaultTakePerPage);
            }
            ViewBag.ActivePage = page;
            ViewBag.Pages = benefitcards.Count();
            return View(benefitcards.ToList());
        }

       // GET: /Admin/Cards/Create
        public ActionResult CreateOrUpdate(string id)
        {
            var card = db.BenefitCards.Find(id) ?? new BenefitCard();
            return View(card);
        }

        [HttpPost]
        public ActionResult CreateOrUpdate(BenefitCard benefitcard)
        {
            if (ModelState.IsValid)
            {
                if (db.BenefitCards.Any(entry => entry.Id == benefitcard.Id))
                {
                    db.Entry(benefitcard).State = EntityState.Modified;
                }
                else
                {
                    db.BenefitCards.Add(benefitcard);    
                }
                db.SaveChanges();
                TempData["SuccessMessage"] = string.Format("Картку або брелок №{0} збережено", benefitcard.Id);
                return RedirectToAction("Index");
            }

            return View(benefitcard);
        }

        public ActionResult Delete(string id)
        {
            var benefitcard = db.BenefitCards.Find(id);
            db.BenefitCards.Remove(benefitcard);
            db.SaveChanges();
            TempData["SuccessMessage"] = string.Format("Картку або брелок №{0} видалено", benefitcard.Id);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
