using System;
using System.Data.Entity;
using System.Web.Mvc;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using System.Linq;
using Benefit.Domain.Models;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Models.Admin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WebGrease.Css.Extensions;

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
            var reusult = users.ToList();
            return PartialView("_UsersSearch", users);
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
        public ActionResult Edit(string id)
        {
            var user = db.Users.Include("Referal").FirstOrDefault(entry => entry.Id == id);
            var userViewModel = new EditUserViewModel()
            {
                Id = user.Id,
                ExternalNumber = user.ExternalNumber,
                FullName = user.FullName,
                ReferalNumber = user.Referal == null ? 0 : user.Referal.ExternalNumber,
                ReferalId = user.Referal == null ? null : user.Referal.Id,
                CardNumber = user.CardNumber,
                NFCNumber = user.NFCCardNumber,
                BusinessLevel = user.BusinessLevel,
                Status = user.Status,
                IsActive = user.IsActive,
                IsCardVerified = user.IsCardVerified,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RegisteredOn = user.RegisteredOn,
                Addresses = user.Addresses,
                BenefitCardOrders = new PaginatedList<Order>
                {
                    Items = db.Orders.Where(entry => entry.UserId == id && entry.OrderType == OrderType.BenefitCard).OrderByDescending(entry=>entry.Time).ToList()
                }
            };
            return View(userViewModel);
        }

        [HttpPost]
        public ActionResult Edit(EditUserViewModel user, string newPassword)
        {
            ApplicationUser referalUser = null;
            if (user.ReferalNumber != null && user.ReferalNumber != 0)
            {
                referalUser = db.Users.FirstOrDefault(entry => entry.ExternalNumber == user.ReferalNumber);
                if (referalUser == null)
                    ModelState.AddModelError("ReferalNumber", "Користувача з таким ID не знайдено");
            }
            if (db.Users.Any(entry => entry.Email == user.Email && entry.Id != user.Id))
            {
                ModelState.AddModelError("Email", "Такий Email вже існує");
            }
            if (db.Users.Any(entry => entry.PhoneNumber == user.PhoneNumber && entry.Id != user.Id))
            {
                ModelState.AddModelError("PhoneNumber", "Такий телефон вже існує");
            }

            if (ModelState.IsValid)
            {
                var existingUser = db.Users.FirstOrDefault(entry => entry.Id == user.Id);
                if (!string.IsNullOrEmpty(newPassword))
                {
                    existingUser.PasswordHash = UserManager.PasswordHasher.HashPassword(newPassword);
                }

                existingUser.FullName = user.FullName;
                existingUser.Referal = referalUser;
                existingUser.NFCCardNumber = user.NFCNumber;
                existingUser.CardNumber = user.CardNumber;
                existingUser.BusinessLevel = user.BusinessLevel;
                existingUser.Status = user.Status;
                existingUser.IsActive = user.IsActive;
                existingUser.IsCardVerified = user.IsCardVerified;
                existingUser.Email = user.Email;
                existingUser.UserName = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;

                db.Entry(existingUser).State = EntityState.Modified;

                user.Addresses.ForEach(entry => entry.UserId = existingUser.Id);
                //to add
                var addressesToAdd = user.Addresses.Where(entry => entry.Id == null).ToList();
                addressesToAdd.ForEach(entry => entry.Id = Guid.NewGuid().ToString());
                db.Addresses.AddRange(addressesToAdd);
                //to edit
                foreach (var address in user.Addresses.Except(addressesToAdd))
                {
                    db.Entry(address).State = EntityState.Modified;
                }
                //to remove
                var addressesToRemove =
                    existingUser.Addresses.Where(entry => !user.Addresses.Select(addr => addr.Id).Contains(entry.Id)).ToList();
                db.Addresses.RemoveRange(addressesToRemove);

                db.SaveChanges();

                TempData["SuccessMessage"] = "Дані партнера збережено";
                return RedirectToAction("Edit", new { id = existingUser.Id });
            }
            return View(user);
        }

        public ActionResult GetNfcCode(string cardNumber, string userId)
        {
            if (db.Users.Any(entry => entry.CardNumber == cardNumber && entry.Id != userId))
            {
                return Json("occupied", JsonRequestBehavior.AllowGet);
            }

            var card = db.BenefitCards.Find(cardNumber);
            return Json(card == null ? "" : card.NfcCode, JsonRequestBehavior.AllowGet);
        }
    }
}