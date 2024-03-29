﻿using Benefit.Common.Constants;
using Benefit.Common.Helpers;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Services.Domain;
using Benefit.Web.Controllers.Base;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;
using Benefit.Web.Models;
using Facebook;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Benefit.Web.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        public AccountController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
            UserManager.PasswordValidator = new MinimumLengthValidator(3);
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
            var provider = new DpapiDataProtectionProvider("ASP.NET IDENTITY");
            UserManager.EmailService = new EmailService();
            UserManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(
                provider.Create("EmailConfirmation"))
            {
                TokenLifespan = TimeSpan.FromDays(30)
            };
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
            {
                AllowOnlyAlphanumericUserNames = false
            };
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        [FetchLastNews]
        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public ActionResult Login(string returnUrl, string id)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Referal = id;
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                var viewName = string.Format("~/views/sellerarea/{0}/Login.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return View(viewName, seller);
            }
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [FetchLastNews]
        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl, bool isAjaxRequest = false)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    if (!user.EmailConfirmed)
                    {
                        ModelState.AddModelError("", "Email не підтверджено");
                    }
                    else
                    {
                        await SignInAsync(user, model.RememberMe);
                        var productsService = new ProductsService();
                        CookiesService.Instance.AddCookie("favoritesNumber", productsService.GetFavorites(user.Id).Count.ToString());
                        if (isAjaxRequest)
                        {
                            return Json(new { returnUrl }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return RedirectToLocal(returnUrl);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Невірний логін чи пароль");
                }
            }

            if (isAjaxRequest)
            {
                return Json(new { error = ModelState.ModelStateErrors() }, JsonRequestBehavior.AllowGet);
            }
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                var viewName = string.Format("~/views/sellerarea/{0}/Login.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return View(viewName, seller);
            }
            return View(model);
        }

        //
        // GET: /Account/Register
        //[AllowAnonymous]
        //public ActionResult Register(string id = null)
        //{
        //    int? ReferalNumber = (id != null) ? int.Parse(id) : (int?)null;
        //    if (ReferalNumber == null)
        //    {
        //        ReferalNumber = CookiesService.Instance.GetCookieValue<int>(RouteConstants.ReferalCookieName);
        //        if (ReferalNumber == 0)
        //        {
        //            ReferalNumber = null;
        //        }
        //    }
        //    else
        //    {
        //        CookiesService.Instance.AddCookie(RouteConstants.ReferalCookieName, ReferalNumber.ToString());
        //    }
        //    return View(new RegisterViewModel { ReferalNumber = ReferalNumber });
        //}

        [AllowAnonymous]
        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        public ActionResult PostRegister()
        {
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                var viewName = string.Format("~/views/sellerarea/{0}/PostRegister.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return View(viewName);
            }
            return View();
        }

        [AllowAnonymous]
        [FetchCategories(Order = 1)]
        [HttpPost]
        public ActionResult PostRegister(string email)
        {
            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [FetchLastNews]
        [FetchSeller(Order = 0)]
        [FetchCategories(Order = 1)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, string returnUrl, bool isAjaxRequest = false, bool getUserIdIfExists = false)
        {
            var existingUserWarning = false;
            if (isAjaxRequest)
            {
                ModelState.Remove("model.Password");
                model.Password = StringHelper.RandomString(8);
            }
            if (model.FullName != null && model.FullName.Trim().Split(' ').Length < 2)
            {
                ModelState.AddModelError("FullName", "Ім'я та прізвище є обов'язковими для заповнення");
            }
            ApplicationUser referal;
            var externalNumber = SettingsService.MinUserExternalNumber;
            BenefitCard card;
            using (var db = new ApplicationDbContext())
            {
                card = db.BenefitCards.FirstOrDefault(entry => entry.Id == model.CardNumber);
                externalNumber = db.Users.Max(entry => entry.ExternalNumber);
                referal = db.Users.FirstOrDefault(entry => entry.ExternalNumber == model.ReferalNumber);
                if (model.CardNumber != null && card == null)
                {
                    ModelState.AddModelError("CardNumber", "Такої картки не існує");
                }
                if (db.Users.Any(entry => entry.CardNumber != null && entry.CardNumber == model.CardNumber))
                {
                    ModelState.AddModelError("CardNumber", "Ця картка зайнята");
                }
                if (model.ReferalNumber != null && referal == null)
                {
                    ModelState.AddModelError("ReferalNumber", "Користувача з таким реферальним кодом не знайдено");
                }

                var user = db.Users.FirstOrDefault(entry => entry.Email == model.Email);
                if (user != null)
                {
                    if (isAjaxRequest)
                    {
                        if (getUserIdIfExists)
                        {
                            return Json(new { returnUrl, string.Empty, userId = user.Id }, JsonRequestBehavior.AllowGet);
                        }
                        ModelState.AddModelError("Email", "Покупець з цією адресою ел.пошти вже зареєстрований! <a href='#' class='x-pseudo-link login-link'>Увійдіть з паролем</a> і ми збережемо це замовлення у Вашому особистому кабінеті");
                        existingUserWarning = true;
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "Цей Email вже зареєстрований");
                    }
                }

                user = db.Users.FirstOrDefault(entry => entry.PhoneNumber == model.PhoneNumber);
                if (user != null)
                {
                    if (isAjaxRequest)
                    {
                        if (getUserIdIfExists)
                        {
                            return Json(new { returnUrl, string.Empty, userId = user.Id }, JsonRequestBehavior.AllowGet);
                        }
                        ModelState.AddModelError("Email", "Покупець з цим номером телефону вже зареєстрований! <a href='#' class='x-pseudo-link login-link'>Увійдіть з паролем</a> і ми збережемо це замовлення у Вашому особистому кабінеті");
                        existingUserWarning = true;
                    }
                    else
                    {
                        ModelState.AddModelError("Phone", "Цей телефон вже зареєстрований");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = model.Email,
                    FullName = model.FullName.Trim(),
                    RegionId = model.RegionId.GetValueOrDefault(RegionConstants.AllUkraineRegionId),
                    Email = model.Email,
                    IsActive = true,
                    ExternalNumber = ++externalNumber,
                    CardNumber = model.CardNumber,
                    NFCCardNumber = card == null ? null : card.NfcCode,
                    PhoneNumber = model.PhoneNumber,
                    RegisteredOn = DateTime.UtcNow
                };
                if (referal == null)
                {
                    var UserService = new UserService();
                    //user.ReferalId = UserService.GetVipReferalCode();
                }
                else
                {
                    user.ReferalId = referal.Id;
                }

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("confirmEmail", "account",
                       new { userId = user.Id, code }, protocol: Request.Url.Scheme);
                    var emailText = isAjaxRequest
                        ? string.Format(
                            "Вітаємо на сайті Benefit Company! <br/> Ваш акаунт було створено автоматично, ваш пароль <b>{0}</n>.<br>Будь ласка підтвердіть реєстрацію, натиснувши на <a href=\"{1}\">це посилання</a>",
                            model.Password, callbackUrl)
                        : string.Format(
                            "Будь ласка підтвердіть реєстрацію, натиснувши на <a href=\"{0}\">це посилання</a>",
                            callbackUrl);
                    await UserManager.SendEmailAsync(user.Id, "Реєстрація на сайті Benefit Company", emailText);
                    var userService = new UserService();
                    userService.SubscribeSendPulse(user.Email);
                    //if shipping address provided - create db entry
                    Address shippingAddress = null;
                    if (!string.IsNullOrEmpty(model.ShippingAddress))
                    {
                        shippingAddress = new Address()
                        {
                            Id = Guid.NewGuid().ToString(),
                            IsDefault = true,
                            AddressLine = model.ShippingAddress,
                            FullName = model.FullName,
                            Phone = model.PhoneNumber,
                            RegionId = model.RegionId.Value,
                            UserId = user.Id
                        };
                        using (var db = new ApplicationDbContext())
                        {
                            db.Addresses.Add(shippingAddress);
                            db.SaveChanges();
                        }
                    }

                    TempData["RegisteredEmail"] = user.Email;
                    if (isAjaxRequest)
                    {
                        return Json(new { returnUrl, shippingAddressId = shippingAddress == null ? string.Empty : shippingAddress.Id, userId = user.Id }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return RedirectToAction("postregister", "account");
                    }
                }
                else
                {
                    AddErrors(result);
                }
            }
            if (isAjaxRequest)
            {
                return Json(new { error = ModelState.ModelStateErrors(), existingUserWarning }, JsonRequestBehavior.AllowGet);
            }
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                var viewName = string.Format("~/views/sellerarea/{0}/Login.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return View(viewName, seller);
            }
            return View("Login", model);
        }

        [FetchCategories(Order = 1)]
        [HttpGet]
        [AllowAnonymous]
        [FetchSeller]
        public async Task<ActionResult> ForgotPassword()
        {
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                var viewName = string.Format("~/views/sellerarea/{0}/ForgotPassword.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return View(viewName);
            }
            return View();
        }

        [FetchCategories(Order = 1)]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [FetchSeller]
        public async Task<ActionResult> ForgotPassword(string Email)
        {
            var user = await UserManager.FindByNameAsync(Email);
            if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");
            }

            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            var callbackUrl = Url.Action("resetPassword", "account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(user.Id, "Reset Password", "Для відновлення паролю натисніть <a href=\"" + callbackUrl + "\">ТУТ</a>");
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                var viewName = string.Format("~/views/sellerarea/{0}/ForgotPasswordConfirmation.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return View(viewName);
            }
            return View("ForgotPasswordConfirmation");
        }

        //
        // GET: /Account/ResetPassword
        [FetchCategories(Order = 1)]
        [HttpGet]
        [AllowAnonymous]
        [FetchSeller]
        public ActionResult ResetPassword(string userId, string code = null)
        {
            if (code == null)
                return View("Error");
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                var viewName = string.Format("~/views/sellerarea/{0}/ResetPassword.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return View(viewName, new ResetPasswordViewModel
                {
                    Id = userId
                });
            }
            return View("ResetPassword", new ResetPasswordViewModel
            {
                Id = userId
            });
        }

        //
        // POST: /Account/ResetPassword
        [FetchCategories(Order = 1)]
        [HttpPost]
        [AllowAnonymous]
        [FetchSeller]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var viewName = "ResetPassword";
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                viewName = string.Format("~/views/sellerarea/{0}/ResetPasswordConfirmation.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
            }
            if (!ModelState.IsValid)
            {
                return View(viewName, model);
            }
            var user = await UserManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [FetchSeller]
        [FetchCategories(Order = 1)]
        public ActionResult ResetPasswordConfirmation()
        {
            var seller = ViewBag.Seller as Seller;
            if (seller != null)
            {
                var viewName = string.Format("~/views/sellerarea/{0}/ResetPasswordConfirmation.cshtml",
                    seller.EcommerceTemplate.GetValueOrDefault(SellerEcommerceTemplate.Default));
                return View(viewName);
            }
            return View();
        }

        [FetchCategories(Order = 1)]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded)
            {
                return View("ConfirmEmail");
            }
            AddErrors(result);
            return View();
        }

        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            HttpContext.Session.RemoveAll();
            Session["Workaround"] = 0;
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        [FetchCategories]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                if (loginInfo.Email != null)
                {
                    user = await UserManager.FindByNameAsync(loginInfo.Email);
                }
                if (user != null)
                {
                    try
                    {
                        var addLoginResult = await UserManager.AddLoginAsync(user.Id, loginInfo.Login);
                        if (addLoginResult.Succeeded)
                        {
                            await SignInAsync(user, isPersistent: false);
                            return RedirectToLocal(returnUrl);
                        }
                    }
                    catch
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;

                var referalCookie = System.Web.HttpContext.Current.Request.Cookies[RouteConstants.ReferalCookieName];
                var referalNumber = 0;
                ExternalLoginConfirmationViewModel model = null;
                if (referalCookie != null)
                {
                    int.TryParse(referalCookie.Value, out referalNumber);
                }
                if (loginInfo.Login.LoginProvider == "Facebook")
                {
                    var authSession = loginInfo.ExternalIdentity.Claims.First(entry => entry.Type == "FacebookAccessToken");
                    var client = new FacebookClient(authSession.Value);
                    dynamic fbresult = client.Get("me?fields=id,email,name");
                    FacebookUserModel facebookUser = JsonConvert.DeserializeObject<FacebookUserModel>(fbresult.ToString());
                    model = new ExternalLoginConfirmationViewModel { Email = facebookUser.email, FullName = facebookUser.name, ReferalNumber = referalNumber };
                }
                if (loginInfo.Login.LoginProvider == "Google")
                {
                    model = new ExternalLoginConfirmationViewModel { Email = loginInfo.Email, FullName = loginInfo.ExternalIdentity.Name, ReferalNumber = referalNumber };
                }
                return View("ExternalLoginConfirmation", model);
            }
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [FetchCategories]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }
            int externalNumber;
            ApplicationUser referal = null;
            using (var db = new ApplicationDbContext())
            {
                externalNumber = db.Users.Max(entry => entry.ExternalNumber);
                referal = db.Users.FirstOrDefault(entry => entry.ExternalNumber == model.ReferalNumber);
            }
            if (model.ReferalNumber != null && referal == null)
            {
                ModelState.AddModelError("ReferalNumber", "Користувача з таким реферальним кодом не знайдено");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser()
                {
                    ReferalId = referal == null ? null: referal.Id,
                    UserName = model.Email,
                    Email = model.Email,
                    IsActive = true,
                    ExternalNumber = ++externalNumber,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    RegisteredOn = DateTime.UtcNow,
                    RegionId = model.RegionId.Value
                };

                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            CookiesService.Instance.RemoveCookie(RouteConstants.FullNameCookieName);
            CookiesService.Instance.RemoveCookie(RouteConstants.SelfReferalCookieName);
            CookiesService.Instance.RemoveCookie(DomainConstants.SellerSessionIdKey);
            CookiesService.Instance.RemoveCookie("favoritesNumber");
            Session.Remove(DomainConstants.SellerSessionIdKey);
            Session.Remove(DomainConstants.SellerSessionNameKey);
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FullName));
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        [AllowAnonymous]
        public ActionResult GetReferalName(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var referal = db.Users.FirstOrDefault(entry => entry.ExternalNumber.ToString() == id);
                if (referal != null)
                {
                    return Content(referal.FullName);
                }
                return null;
            }
        }

        [AllowAnonymous]
        public ActionResult CheckRegisteredCard(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var card = db.BenefitCards.Include(entry => entry.ReferalUser).FirstOrDefault(entry => entry.Id == id);
                if (card != null)
                {
                    return
                        Content(
                            JsonConvert.SerializeObject(new { card.ReferalUser.ExternalNumber, card.ReferalUser.FullName }),
                            "application/json");
                }
                throw new HttpException(404, "Not found");
            }
        }

        public async Task<ActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), currentPassword, newPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Пароль успішно змінено";
            }
            else
            {
                AddErrors(result);
                TempData["ErrorMessage"] = ModelState.ModelStateErrors();
            }
            return RedirectToAction("Profile", "Panel", new { area = DomainConstants.CabinetAreaName });
        }
    }
}