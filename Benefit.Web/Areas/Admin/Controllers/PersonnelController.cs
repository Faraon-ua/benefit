using System;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class PersonnelController : AdminController
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly UserManager<ApplicationUser> _userManager;
        public PersonnelController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public PersonnelController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

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
        public ActionResult CreateModerator(Personnel personnel)
        {
            personnel.Id = Guid.NewGuid().ToString();
            if (ModelState.IsValid)
            {
                _userManager.AddToRole(personnel.UserId, personnel.RoleName);
                db.Personnels.Add(personnel);
                db.SaveChanges();
            }
            var sellerPersonnel = db.Personnels.Where(entry => entry.SellerId == personnel.SellerId).ToList();
            return PartialView("_PersonnelList", sellerPersonnel);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var personnel = db.Personnels.Find(id);
            if (personnel.RoleName!= null)
            {
                _userManager.RemoveFromRole(personnel.UserId, personnel.RoleName);
            }
            var sellerPersonnel = db.Personnels.Where(entry => entry.SellerId == personnel.SellerId && entry.Id != personnel.Id).ToList();
            db.Personnels.Remove(personnel);
            db.SaveChanges();
            return PartialView("_PersonnelList", sellerPersonnel);
        }
    }
}
