using System;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Web.Areas.Admin.Controllers.Base;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class PersonnelController : AdminController
    {
        ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        public ActionResult Create(Personnel personnel)
        {
            personnel.Id = Guid.NewGuid().ToString();
            var card = db.BenefitCards.FirstOrDefault(entry => entry.Id == personnel.CardNumber);
            personnel.NFCCardNumber = card == null ? null : card.NfcCode;
            if (ModelState.IsValid)
            {
                db.Personnels.Add(personnel);
                db.SaveChanges();
            }
            var sellerPersonnel = db.Personnels.Where(entry => entry.SellerId == personnel.SellerId);
            return PartialView("_PersonnelList", sellerPersonnel.ToList());
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var personnel = db.Personnels.Find(id);
            var sellerPersonnel = db.Personnels.Where(entry => entry.SellerId == personnel.SellerId && entry.Id != personnel.Id).ToList();
            db.Personnels.Remove(personnel);
            db.SaveChanges();
            return PartialView("_PersonnelList", sellerPersonnel);
        }
    }
}
