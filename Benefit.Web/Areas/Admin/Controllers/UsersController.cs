using Benefit.Common.Constants;
using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;
using Benefit.Services.Domain;
using Benefit.Web.Areas.Admin.Controllers.Base;
using Benefit.Web.Models.Admin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebGrease.Css.Extensions;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.AdminRoleName)]
    public class UsersController : AdminController
    {
        public UsersController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())),
            new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext())))
        {
        }
        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            UserManager = userManager;
            RolesManager = roleManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }
        public RoleManager<IdentityRole> RolesManager { get; private set; }
        //
        // GET: /Admin/Users/
        public ActionResult Index()
        {
            using (var db = new ApplicationDbContext())
            {
                var usersCount = db.Users.Count();
                return View(usersCount);
            }
        }

        public ActionResult UsersSearch(UserFilterValues filters)
        {
            using (var db = new ApplicationDbContext())
            {
                var users = db.Users.Include(entry => entry.Referal).Include(entry => entry.BenefitCards).AsQueryable();
                if (!string.IsNullOrEmpty(filters.DateRange))
                {
                    var dateRangeValues = filters.DateRange.Split('-');
                    var startDate = DateTime.Parse(dateRangeValues.First());
                    var endDate = DateTime.Parse(dateRangeValues.Last());
                    users = users.Where(entry => entry.RegisteredOn >= startDate && entry.RegisteredOn <= endDate);
                }
                if (filters.MinStatusCompletions.HasValue)
                {
                    users = users.Where(entry => entry.StatusCompletionMonths >= filters.MinStatusCompletions.Value);
                }
                if (!string.IsNullOrEmpty(filters.Search))
                {
                    var search = filters.Search;
                    search = search.ToLower();
                    users = users.Where(entry => entry.FullName.ToLower().Contains(search) ||
                                                 entry.Email.ToLower().Contains(search) ||
                                                 entry.PhoneNumber.ToLower().Contains(search) ||
                                                 entry.UserName.ToLower().Contains(search) ||
                                                 entry.CardNumber.ToString().Contains(search) ||
                                                 entry.ExternalNumber.ToString().Contains(search) ||
                                                 entry.BenefitCards.Any(bc => bc.Id == search));
                }
                return PartialView("_UsersSearch", users.OrderByDescending(entry => entry.RegisteredOn).ToList());
            }
        }
        [HttpPost]
        public ActionResult LockUnlock(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var existingUser = db.Users.FirstOrDefault(entry => entry.Id == id);
                existingUser.IsActive = !existingUser.IsActive;
                db.Entry(existingUser).State = EntityState.Modified;
                db.SaveChanges();
                return Json(existingUser.IsActive);
            }
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
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users
                    .Include(entry=>entry.Referal)
                    .Include(entry=>entry.Addresses.Select(add=>add.Region.Parent.Parent))
                    .FirstOrDefault(entry => entry.Id == id);
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
                    StatusCompletionMonths = user.StatusCompletionMonths,
                    IsActive = user.IsActive,
                    IsCardVerified = user.IsCardVerified,
                    EmailConfirmed = user.EmailConfirmed,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RegisteredOn = user.RegisteredOn,
                    Addresses = user.Addresses,
                    BonusAccount = user.BonusAccount.ToDoubleDigitsNumber(),
                    HangingBonusAccount = user.HangingBonusAccount.ToDoubleDigitsNumber(),
                };
                var userRoles = UserManager.GetRoles(id);
                userViewModel.Roles =
                    RolesManager.Roles.Select(
                        entry => new SelectListItem() { Text = entry.Name, Selected = userRoles.Contains(entry.Name) })
                        .OrderBy(entry => entry.Text).ToList();
                return View(userViewModel);
            }
        }
        [HttpPost]
        public ActionResult Edit(EditUserViewModel user, string newPassword)
        {
            using (var db = new ApplicationDbContext())
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
                if (db.Users.Any(entry => entry.CardNumber != null && entry.CardNumber == user.CardNumber && entry.Id != user.Id))
                {
                    ModelState.AddModelError("CardNumber", "Така картка зайнята");
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
                    existingUser.IsActive = user.IsActive;
                    existingUser.StatusCompletionMonths = user.StatusCompletionMonths;

                    //if card verivied - set partner status
                    if (user.IsCardVerified && !existingUser.IsCardVerified && existingUser.Status == null)
                    {
                        existingUser.Status = Status.Partner;
                    }
                    else
                    {
                        existingUser.Status = user.Status;
                    }
                    existingUser.IsCardVerified = user.IsCardVerified;
                    existingUser.EmailConfirmed = user.EmailConfirmed;
                    existingUser.Email = user.Email;
                    existingUser.UserName = user.Email;
                    existingUser.PhoneNumber = user.PhoneNumber;

                    existingUser.BonusAccount = user.BonusAccount;
                    existingUser.HangingBonusAccount = user.HangingBonusAccount;

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

                    if (UserManager.IsInRole(User.Identity.GetUserId(), DomainConstants.SuperAdminRoleName))
                    {
                        foreach (var role in user.Roles)
                        {
                            if (role.Selected && !UserManager.IsInRole(user.Id, role.Text))
                            {
                                UserManager.AddToRole(user.Id, role.Text);
                            }
                            if (!role.Selected && UserManager.IsInRole(user.Id, role.Text))
                            {
                                UserManager.RemoveFromRole(user.Id, role.Text);
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "Дані партнера збережено";
                    return RedirectToAction("Edit", new { id = existingUser.Id });
                }
                var userRoles = UserManager.GetRoles(user.Id);
                user.Roles =
                    RolesManager.Roles.Select(
                        entry => new SelectListItem() { Text = entry.Name, Selected = userRoles.Contains(entry.Name) })
                        .OrderBy(entry => entry.Text).ToList();
                return View(user);
            }
        }

        public ActionResult GetNfcCode(string cardNumber, string userId)
        {
            using (var db = new ApplicationDbContext())
            {
                if (db.Users.Any(entry => entry.CardNumber == cardNumber && entry.Id != userId))
                {
                    return Json("occupied", JsonRequestBehavior.AllowGet);
                }

                var card = db.BenefitCards.FirstOrDefault(entry => entry.Id == cardNumber);
                return Json(card == null ? "" : card.NfcCode, JsonRequestBehavior.AllowGet);
            }
        }
    }
}