﻿using System;
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
    [Authorize(Roles = DomainConstants.AdminRoleName + "," + DomainConstants.SellerRoleName)]
    public class PersonnelController : AdminController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public PersonnelController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public PersonnelController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }
        public ActionResult GetUserByExternalNumber(int externalNumber)
        {
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.FirstOrDefault(entry => entry.ExternalNumber == externalNumber);
                if (user == null) return null;
                return Json(new { user.Id, user.FullName, user.PhoneNumber }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(Personnel personnel)
        {
            using (var db = new ApplicationDbContext())
            {
                personnel.Id = Guid.NewGuid().ToString();
                var card = db.BenefitCards.FirstOrDefault(entry => entry.Id == personnel.CardNumber);
                personnel.NFCCardNumber = card == null ? null : card.NfcCode;
                if (personnel.NFCCardNumber != null)
                {
                    ModelState["NFCCardNumber"].Errors.Clear();
                }
                if (db.Personnels.Any(entry => entry.NFCCardNumber == personnel.NFCCardNumber))
                {
                    ModelState.AddModelError("NFCCardNumber", "Дана картка вже зайнята");
                }
                if (ModelState.IsValid)
                {
                    db.Personnels.Add(personnel);
                    db.SaveChanges();
                    var sellerPersonnel = db.Personnels.Where(entry => entry.SellerId == personnel.SellerId);
                    return PartialView("_PersonnelList", sellerPersonnel.ToList());
                }
                return
                    Json(
                        new
                        {
                            error =
                                string.Join("<br/>",
                                    ModelState.SelectMany(entry => entry.Value.Errors).Select(entry => entry.ErrorMessage).ToList())
                        });
            }
        }

        [HttpPost]
        public ActionResult CreateModerator(Personnel personnel)
        {
            using (var db = new ApplicationDbContext())
            {
                personnel.Id = Guid.NewGuid().ToString();
                ModelState["NFCCardNumber"].Errors.Clear();
                personnel.NFCCardNumber = "dummy";
                if (ModelState.IsValid)
                {
                    _userManager.AddToRole(personnel.UserId, personnel.RoleName);
                    db.Personnels.Add(personnel);
                    db.SaveChanges();
                }
                var sellerPersonnel = db.Personnels.Where(entry => entry.SellerId == personnel.SellerId).ToList();
                return PartialView("_PersonnelList", sellerPersonnel);
            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var personnel = db.Personnels.Find(id);
                if (personnel.RoleName != null)
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
}
