﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class PersonnelController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        public ActionResult Create(Personnel personnel)
        {
            personnel.Id = Guid.NewGuid().ToString();
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
