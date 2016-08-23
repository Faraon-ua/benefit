using System;
using System.Data.Entity;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using System.Linq;
using Benefit.Domain.Models;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Models.Admin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class UsersController : AdminController
    {
        public ApplicationDbContext db = new ApplicationDbContext();
        public UsersController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }
        public UsersController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }
        //
        // GET: /Admin/Users/
        public ActionResult Index(string search)
        {
            var usersCount = db.Users.Count();
            return View(usersCount);
        }

        public ActionResult UsersSearch(string search, string dateRange = null)
        {
            IQueryable<ApplicationUser> users = db.Users.AsQueryable();
            if (!string.IsNullOrEmpty(dateRange))
            {
                var dateRangeValues = dateRange.Split('-');
                var startDate = DateTime.Parse(dateRangeValues.First());
                var endDate = DateTime.Parse(dateRangeValues.Last());
                users = users.Where(entry => entry.RegisteredOn >= startDate && entry.RegisteredOn <= endDate);
            }
            else
            {
                search = search.ToLower();
                users = users.Where(entry => entry.FullName.ToLower().Contains(search) ||
                                             entry.Email.ToLower().Contains(search) ||
                                             entry.PhoneNumber.ToLower().Contains(search) ||
                                             entry.UserName.ToLower().Contains(search) ||
                                             entry.CardNumber.ToString().Contains(search) ||
                                             entry.ExternalNumber.ToString().Contains(search));
            }
            return PartialView("_UsersSearch", users);
        }

        public ActionResult Edit(string id)
        {
            var user = db.Users.Include("Referal").FirstOrDefault(entry => entry.Id == id);
            var userViewModel = new EditUserViewModel()
            {
                ExternalNumber = user.ExternalNumber,
                FullName = user.FullName,
                ReferalNumber = user.Referal == null ? 0 : user.Referal.ExternalNumber,
                ReferalId = user.Referal == null ? null : user.Referal.Id,
                CardNumber = user.CardNumber,
                BusinessLevel = user.BusinessLevel,
                Status = user.Status,
                IsActive = user.IsActive,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RegisteredOn = user.RegisteredOn
            };
            return View(userViewModel);
        }

        [HttpPost]
        public ActionResult LockUnlock(string id)
        {
            var existingUser = db.Users.FirstOrDefault(entry => entry.Id == id);
            existingUser.IsActive = !existingUser.IsActive;
            db.Entry(existingUser).State = EntityState.Modified;
            db.SaveChanges();
            return Json(existingUser.IsActive);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var user = UserManager.FindById(id);
            UserManager.Delete(user);
            return Json(true);
        }

        [HttpPost]
        public ActionResult Edit(EditUserViewModel user, string newPassword)
        {
            ApplicationUser referalUser = null;
            if (user.ReferalNumber != 0)
            {
                referalUser = db.Users.FirstOrDefault(entry => entry.ExternalNumber == user.ReferalNumber);
            }
            if (referalUser == null) 
                ModelState.AddModelError("ReferalNumber", "Користувача з таким ID не знайдено");
            if (ModelState.IsValid)
            {
                var existingUser = db.Users.FirstOrDefault(entry => entry.Id == user.Id);
                if (!string.IsNullOrEmpty(newPassword))
                {
                    existingUser.PasswordHash = UserManager.PasswordHasher.HashPassword(newPassword);
                }

                existingUser.FullName = user.FullName;
                existingUser.Referal = referalUser;
                existingUser.CardNumber = user.CardNumber;
                existingUser.BusinessLevel = user.BusinessLevel;
                existingUser.Status = user.Status;
                existingUser.IsActive = user.IsActive;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;

                db.Entry(existingUser).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Дані партнера збережено";
                return RedirectToAction("Edit", new { id = existingUser.Id });
            }
            return View(user);
        }
    }
}