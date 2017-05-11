using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Services.Domain;
using Facebook;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Benefit.Web.Models;
using Microsoft.Owin.Security.DataProtection;

namespace Benefit.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
            UserManager.PasswordValidator = new MinimumLengthValidator(3);
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
            var provider = new DpapiDataProtectionProvider("Sample");
            UserManager.EmailService = new EmailService();
            UserManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(
                provider.Create("EmailConfirmation"));
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
            {
                AllowOnlyAlphanumericUserNames = false
            };
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Невірний логін чи пароль");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string id = null)
        {
            int? ReferalNumber = (id != null) ? int.Parse(id) : (int?)null;
            if (ReferalNumber == null)
            {
                ReferalNumber = CookiesService.Instance.GetCookieValue<int>(RouteConstants.ReferalCookieName);
                if (ReferalNumber == 0) ReferalNumber = null;
            }
            else
            {
                CookiesService.Instance.AddCookie(RouteConstants.ReferalCookieName, ReferalNumber.ToString());
            }
            return View(new RegisterViewModel { ReferalNumber = ReferalNumber });
        }

        [AllowAnonymous]
        public ActionResult PostRegister()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult PostRegister(string email)
        {
            var userService = new UserService();
            userService.SubscribeSendPulse(email);
            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            ApplicationUser referal;
            var externalNumber = SettingsService.MinUserExternalNumber;
            BenefitCard card;
            using (var db = new ApplicationDbContext())
            {
                card = db.BenefitCards.FirstOrDefault(entry=>entry.Id == model.CardNumber);
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
                if (referal == null)
                {
                    ModelState.AddModelError("ReferalNumber", "Користувача з таким реферальним кодом не знайдено");
                }
                if (db.Users.Any(entry => entry.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Цей Email вже зареєстрований");
                }
                if (db.Users.Any(entry => entry.PhoneNumber == model.PhoneNumber))
                {
                    ModelState.AddModelError("Email", "Цей телефон вже зареєстрований");
                }
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = model.Email,
                    ReferalId = referal.Id,
                    FullName = string.Format("{0} {1}", model.FirstName, model.LastName),
                    RegionId = model.RegionId.GetValueOrDefault(RegionConstants.AllUkraineRegionId),
                    Email = model.Email,
                    IsActive = true,
                    ExternalNumber = ++externalNumber,
                    CardNumber = model.CardNumber,
                    NFCCardNumber = card == null ? null : card.NfcCode,
                    PhoneNumber = model.PhoneNumber,
                    RegisteredOn = DateTime.UtcNow
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                var userService = new UserService();
                userService.SubscribeSendPulse(user.Email);

                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);

                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account",
                       new { userId = user.Id, code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id,
                       "Підтвердження реєстрації на сайті Benefit Company", "Будь ласка підтвердіть реєстрацію, натиснувши на <a href=\""
                       + callbackUrl + "\">це посилання</a>");

                    TempData["RegisteredEmail"] = user.Email;
                    return RedirectToAction("PostRegister", "Account");
                }
                else
                {
                    AddErrors(result);
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(string Email)
        {
            var user = await UserManager.FindByNameAsync(Email);
            if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");
            }

            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(user.Id, "Reset Password", "Для відновлення паролю натисніть <a href=\"" + callbackUrl + "\">ТУТ</a>");
            return View("ForgotPasswordConfirmation");
        }

        //
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword(string userId, string code = null)
        {
            return code == null ? View("Error") : View(new ResetPasswordViewModel
            {
                Id = userId
            });
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
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
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

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
            ViewBag.ReturnUrl = Url.Action("Manage");
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
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
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
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;

                var authSession = loginInfo.ExternalIdentity.Claims.First(entry => entry.Type == "FacebookAccessToken");
                var client = new FacebookClient(authSession.Value);
                dynamic fbresult = client.Get("me?fields=id,email,name");
                FacebookUserModel facebookUser = Newtonsoft.Json.JsonConvert.DeserializeObject<FacebookUserModel>(fbresult.ToString());
                var referalCookie = System.Web.HttpContext.Current.Request.Cookies[RouteConstants.ReferalCookieName];
                var referalNumber = 0;
                if (referalCookie != null)
                {
                    int.TryParse(referalCookie.Value, out referalNumber);
                }
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = facebookUser.email, FullName = facebookUser.name, ReferalNumber = referalNumber });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
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
            if (referal == null)
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
                    ReferalId = referal.Id,
                    UserName = model.UserName,
                    Email = model.UserName,
                    IsActive = false,
                    ExternalNumber = ++externalNumber,
                    CardNumber = model.CardNumber,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    RegisteredOn = DateTime.UtcNow,
                };

                var result = await UserManager.CreateAsync(user);

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
            if (Request.Cookies[RouteConstants.FullNameCookieName] != null)
            {
                CookiesService.Instance.RemoveCookie(RouteConstants.FullNameCookieName);
            }
            if (Request.Cookies[RouteConstants.SelfReferalCookieName] != null)
            {
                CookiesService.Instance.RemoveCookie(RouteConstants.SelfReferalCookieName);
            }
            if (Session[DomainConstants.SellerSessionIdKey] != null)
            {
                CookiesService.Instance.RemoveCookie(DomainConstants.SellerSessionIdKey);
            }
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
    }
}