using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Services.Domain;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;

namespace Benefit.Web.Areas.Cabinet.Controllers
{
    [CabinetFilter]
    public class PanelController : BasePartnerController
    {
        private UserService UserService { get; set; }
        public PanelController()
        {
            UserService = new UserService();
        }

        [HttpPost]
        public ActionResult GetPartnersByReferalIds(string[] ids, int level)
        {
            if (ids == null)
            {
                return Json(new { });
            }
            var partners = UserService.GetPartnersByReferalIds(ids);
            var result = partners.ToDictionary(entry => entry.Key,
                entry => ControllerContext.RenderPartialToString("_PartnersPartial",
                    new KeyValuePair<int, IEnumerable<ApplicationUser>>(level, entry.Value)));
            var jsonResult = Json(result);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        //
        // GET: /Cabinet/Panel/
        public ActionResult Index()
        {
            using (var db = new ApplicationDbContext())
            {
                var banners = db.Banners.Where(entry => entry.BannerType == BannerType.PartnerPageBanners).ToList();
                return View(banners);
            }
        }

        public ActionResult GetOrderDetails(string orderId)
        {
            using (var db = new ApplicationDbContext())
            {
                var order =
                    db.Orders.Include(entry => entry.OrderProducts)
                        .Include(entry => entry.OrderProductOptions)
                        .FirstOrDefault(entry => entry.Id == orderId);
                return PartialView("_OrderDetailsPartial", order);
            }
        }

        public ActionResult Profile()
        {
            var user = ViewBag.User;
            return View(user);
        }

        [HttpPost]
        public ActionResult Profile(UpdateProfileViewModel profile)
        {
            using (var db = new ApplicationDbContext())
            {
                var userId = RouteData.Values[DomainConstants.UserIdKey].ToString();
                var user = db.Users.Include(entry => entry.Region).Include(entry => entry.Addresses).Include(entry => entry.Addresses.Select(addr => addr.Region)).FirstOrDefault(entry => entry.Id == userId);
                user.Region = db.Regions.FirstOrDefault(entry => entry.Id == user.RegionId);

                if (db.Users.Any(entry => entry.CardNumber == profile.CardNumber && entry.Id != user.Id) || !db.BenefitCards.Any(entry => entry.Id == profile.CardNumber))
                {
                    ModelState.AddModelError("CardNumber", "Не можливо підвязати цей номер карти до вашого акаунту");
                }
                if (ModelState.IsValid)
                {
                    if (user.CardNumber == null)
                    {
                        user.NFCCardNumber = db.BenefitCards.Find(profile.CardNumber).NfcCode;
                    }
                    user.CardNumber = profile.CardNumber;
                    user.RegionId = profile.RegionId;
                    user.Address = profile.Address;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Ваші дані збережено";
                }
                return View(user);
            }
        }

        [HttpPost]
        public ActionResult SendProfileChangeRequest(string message)
        {
            var emailService = new EmailService();
            var userUrl = Url.Action("Edit", "Users", new { area = "Admin", id = RouteData.Values[DomainConstants.UserIdKey].ToString() }, Request.Url.Scheme);
            emailService.SendProfileChangeRequest(userUrl, message);
            TempData["SuccessMessage"] = "Запит було надіслано";
            return RedirectToAction("Profile");
        }

        public ActionResult VerifyCard()
        {
            var user = ViewBag.User;
            return View(user);
        }

        [HttpPost]
        public ActionResult VerifyCard(HttpPostedFileBase postedFile)
        {
            if (Request.Files.Count > 0 && Request.Files[0] != null && Request.Files[0].ContentLength != 0)
            {
                var file = Request.Files[0];
                var userUrl = Url.Action("Edit", "Users", new { area = "Admin", id = RouteData.Values[DomainConstants.UserIdKey].ToString() }, Request.Url.Scheme);
                var EmailService = new EmailService();
                EmailService.SendCardVerification(userUrl, file);
                TempData["SuccessMessage"] = "Запит на верифікацію карти було відправлено";
            }
            else
            {
                TempData["ErrorMessage"] = "Не вибрано зображення";
            }
            return RedirectToAction("Profile");
        }

        public ActionResult UserAddress(string id)
        {
            Address address;
            var user = ViewBag.User as ApplicationUser;
            using (var db = new ApplicationDbContext())
            {
                address = db.Addresses.FirstOrDefault(entry => entry.Id == id && entry.UserId == user.Id) ?? new Address();
                if (address.Region != null)
                {
                    var expandedRegionName = address.Region.ExpandedName;
                }
            }
            return View(address);
        }

        [HttpPost]
        public ActionResult UserAddress(Address address)
        {
            var userId = RouteData.Values[DomainConstants.UserIdKey].ToString();
            if (address.RegionId == default(int))
            {
                ModelState.AddModelError("RegionName", "Виберіть місто із випадаючого списку");
            }
            if (ModelState.IsValid)
            {
                using (var db = new ApplicationDbContext())
                {
                    address.UserId = userId;
                    if (address.Id == null)
                    {
                        address.Id = Guid.NewGuid().ToString();
                        db.Addresses.Add(address);
                    }
                    else
                    {
                        db.Entry(address).State = EntityState.Modified; ;
                    }
                    db.SaveChanges();
                }
                return RedirectToAction("Profile");
            }
            var user = ViewBag.User;
            ViewBag.User = user;

            return View(address);
        }

        [HttpPost]
        public ActionResult DeleteUserAddress(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var userAddress = db.Addresses.FirstOrDefault(entry => entry.Id == id);
                db.Addresses.Remove(userAddress);
                db.SaveChanges();
            }
            return Content(string.Empty);
        }

        public ActionResult Structure()
        {
            var user = UserService.GetUserInfoWithPartners(RouteData.Values[DomainConstants.UserIdKey].ToString());
            return View(user);
        }

        public ActionResult planvunagorod()
        {
            var user = ViewBag.User;
            using (var db = new ApplicationDbContext())
            {
                ViewBag.Page = db.InfoPages.FirstOrDefault(entry => entry.UrlName == "plan_vinagorod").Content;
                return View(user);
            }
        }

        public ActionResult finansovui_oblik(string dateRange)
        {
            var transactionsService = new TransactionsService();
            if (dateRange == null)
            {
                dateRange = string.Format("{0} - {1}", new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToShortDateString(),
                    DateTime.Today.ToShortDateString());
            }
            var dateRangeValues = dateRange.Split('-');
            var startDate = DateTime.Parse(dateRangeValues.First());
            var endDate = DateTime.Parse(dateRangeValues.Last()).AddTicks(-1).AddDays(1);
            var model = transactionsService.GetPartnerTransactions(RouteData.Values[DomainConstants.UserIdKey].ToString(), startDate, endDate);
            model.DateRange = dateRange;
            return View(model);
        }

        public ActionResult history()
        {
            var userId = RouteData.Values[DomainConstants.UserIdKey].ToString();
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Include(entry => entry.Orders).FirstOrDefault(entry => entry.Id == userId);
                user.Orders = user.Orders.OrderByDescending(entry => entry.Time).ToList();
                return View(user);
            }
        }

        public ActionResult Zakladu()
        {
            var user = UserService.GetUserInfoWithRegionsAndSellers(RouteData.Values[DomainConstants.UserIdKey].ToString());
            return View(user);
        }

        public ActionResult contact_us()
        {
            var user = UserService.GetUserInfoWithRegionsAndSellers(RouteData.Values[DomainConstants.UserIdKey].ToString());
            return View(user);
        }

        [HttpPost]
        public ActionResult contact_us(string Subject, string Message, string userId)
        {
            var emailService = new EmailService();
            var userUrl = Url.Action("Edit", "Users", new { area = "Admin", id = RouteData.Values[DomainConstants.UserIdKey].ToString() }, Request.Url.Scheme);
            emailService.SendUserFeedback(Subject, Message, userUrl);
            TempData["SuccessMessage"] = "Ваш запит було надіслано";
            return RedirectToAction("contact_us");
        }

        public ActionResult SetAvatar()
        {
            var avatar = Request.Files[0];
            var userId = RouteData.Values[DomainConstants.UserIdKey].ToString();
            if (avatar != null && avatar.ContentLength != 0)
            {
                var fileName = Path.GetFileName(avatar.FileName);
                var dotIndex = fileName.IndexOf('.');
                var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
                var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                var pathString = Path.Combine(originalDirectory, "Images", ImageType.UserAvatar.ToString());
                var imageName = UserService.SetUserPic(userId, fileExt);
                var fullPath = Path.Combine(pathString, imageName);

                avatar.SaveAs(fullPath);
                var imagesService = new ImagesService();
                imagesService.ResizeToSiteRatio(Path.Combine(pathString, fullPath), ImageType.UserAvatar);
            }
            return RedirectToAction("Index");
        }

        public ActionResult RemoveCard()
        {
            UserService.RemoveCard(ViewBag.User.Id);
            return Content(string.Empty);
        }
    }
}