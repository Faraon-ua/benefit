using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Services.Domain;
using Benefit.Web.Helpers;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Areas.Cabinet.Controllers
{
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
            var user = UserService.GetUserInfoWithRegions(User.Identity.GetUserId());
            return View(user);
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
            var user = UserService.GetUserInfoWithRegions(User.Identity.GetUserId());
            return View(user);
        }

        [HttpPost]
        public ActionResult Profile(UpdateProfileViewModel profile)
        {
            using (var db = new ApplicationDbContext())
            {
                var userId = User.Identity.GetUserId();
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

        public ActionResult VerifyCard()
        {
            var userId = User.Identity.GetUserId();
            var user = UserService.GetUserInfoWithRegions(userId);
            return View(user);
        }

        [HttpPost]
        public ActionResult VerifyCard(HttpPostedFileBase postedFile)
        {
            if (Request.Files.Count > 0 && Request.Files[0] != null && Request.Files[0].ContentLength != 0)
            {
                var file = Request.Files[0];
                var userUrl = Url.Action("Edit", "Users", new {area = "Admin", id = User.Identity.GetUserId()}, Request.Url.Scheme);
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
            var userId = User.Identity.GetUserId();
            var user = UserService.GetUserInfoWithRegions(userId);
            ViewBag.User = user;
            Address address;
            using (var db = new ApplicationDbContext())
            {
                address = db.Addresses.FirstOrDefault(entry => entry.Id == id && entry.UserId == userId) ?? new Address();
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
            var userId = User.Identity.GetUserId();
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
            var user = UserService.GetUserInfoWithRegions(userId);
            ViewBag.User = user;

            return View(address);
        }

        public ActionResult Structure()
        {
            var user = UserService.GetUserInfoWithPartners(User.Identity.GetUserId());
            return View(user);
        }

        public ActionResult planvunagorod()
        {
            var user = UserService.GetUserInfoWithRegions(User.Identity.GetUserId());
            using (var db = new ApplicationDbContext())
            {
                ViewBag.Page = db.InfoPages.FirstOrDefault(entry => entry.UrlName == "plan_vinagorod").Content;
                return View(user);
            }
        }

        public ActionResult finansovui_oblik()
        {
            var user = UserService.GetUserInfoWithRegions(User.Identity.GetUserId());
            return View(user);
        }

        public ActionResult history()
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Include(entry => entry.Orders).FirstOrDefault(entry => entry.Id == userId);
                user.Orders = user.Orders.OrderByDescending(entry => entry.Time).ToList();
                return View(user);
            }
        }

        public ActionResult Zakladu()
        {
            var user = UserService.GetUserInfoWithRegionsAndOwnedSellers(User.Identity.GetUserId());
            return View(user);
        }

        public ActionResult SetAvatar()
        {
            var avatar = Request.Files[0];
            var userId = User.Identity.GetUserId();
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
    }
}