using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Common.Extensions;
using Benefit.Domain.Models;
using Benefit.Domain.DataAccess;
using Benefit.Services;
using Benefit.Web.Areas.Admin.Controllers.Base;

namespace Benefit.Web.Areas.Admin.Controllers
{
    public class InfoPagesController : AdminController
    {
        private LocalizationService LocalizationService = new LocalizationService();

        // GET: /Admin/InfoPages/
        public ActionResult Index()
        {
            using (var db = new ApplicationDbContext())
            {
                var pages = Seller.CurrentAuthorizedSellerId != null
                ? db.InfoPages.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId)
                : db.InfoPages.Where(entry => entry.SellerId == null);
                return View(pages.ToList());
            }
        }

        // GET: /Admin/InfoPages/Create
        public ActionResult CreateOrUpdate(string id = null)
        {
            using (var db = new ApplicationDbContext())
            {
                var infoPages = db.InfoPages.Where(entry => entry.Id == id);
                if (Seller.CurrentAuthorizedSellerId != null)
                {
                    infoPages = infoPages.Where(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId);
                }
                var infoPage = infoPages.FirstOrDefault() ?? new InfoPage() { Id = Guid.NewGuid().ToString() };
                infoPage.Localizations = LocalizationService.Get(infoPage, 
                    entry=>entry.Name, 
                    entry=>entry.ShortContent, 
                    entry => entry.Content);
                return View(infoPage);
            }
        }

        public ActionResult TabableContent(string pageUrls)
        {
            using (var db = new ApplicationDbContext())
            {
                var pageUrlNames = pageUrls.Split(',');
                var infoPages = db.InfoPages.Where(entry => pageUrlNames.Contains(entry.UrlName)).OrderBy(entry => entry.Order).ToList();
                return View(infoPages);
            }
        }

        public ActionResult Content(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var infoPage = db.InfoPages.FirstOrDefault(entry => entry.UrlName == id);
                return View(infoPage);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrUpdate(InfoPage infopage)
        {
            using (var db = new ApplicationDbContext())
            {
                if (Seller.CurrentAuthorizedSellerId != null && !infopage.IsNews)
                {
                    if (db.InfoPages.Count(entry => entry.SellerId == Seller.CurrentAuthorizedSellerId && !entry.IsNews) > 4)
                    {
                        TempData["ErrorMessage"] = "Ви не можете створити більшу кількість сторінок";
                        return RedirectToAction("Index");
                    }
                }
                for (var i = 0; i < infopage.Localizations.Count; i++)
                {
                    if (infopage.Localizations[i].ResourceValue == null)
                    {
                        foreach (var key in ModelState.Keys.Where(entry => entry.Contains(string.Format("Localizations[{0}]", i))))
                        {
                            ModelState[key].Errors.Clear();
                        }
                    }
                }
                infopage.UrlName = infopage.Name.Translit();
                if (ModelState.IsValid)
                {
                    if (Seller.CurrentAuthorizedSellerId != null)
                    {
                        infopage.SellerId = Seller.CurrentAuthorizedSellerId;
                    }
                    infopage.LastModified = DateTime.UtcNow;
                    infopage.LastModifiedBy = User.Identity.Name;
                    var logo = Request.Files[0];
                    if (logo != null && logo.ContentLength != 0)
                    {
                        var newsLogo = Request.Files[0];
                        var fileName = Path.GetFileName(newsLogo.FileName);
                        var dotIndex = fileName.IndexOf('.');
                        var fileExt = fileName.Substring(dotIndex, fileName.Length - dotIndex);
                        var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                        var relativePathString = Path.Combine("Images", ImageType.NewsLogo.ToString(), infopage.Id + fileExt);
                        var pathString = Path.Combine(originalDirectory, relativePathString);
                        newsLogo.SaveAs(pathString);
                        infopage.ImageUrl = infopage.Id + fileExt;
                        var imagesService = new ImagesService();
                        imagesService.ResizeToSiteRatio(pathString, ImageType.NewsLogo);
                    }
                    if (db.InfoPages.Any(entry => entry.Id == infopage.Id))
                    {
                        db.Entry(infopage).State = EntityState.Modified;
                    }
                    else
                    {
                        infopage.CreatedOn = DateTime.UtcNow;
                        db.InfoPages.Add(infopage);
                    }
                    LocalizationService.Save(infopage.Localizations);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Сторінку збережено";
                    return RedirectToAction("CreateOrUpdate", new { id = infopage.Id });
                }

                return View(infopage);
            }
        }

        public ActionResult Delete(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var infopage = db.InfoPages.Find(id);
                if (infopage != null)
                {
                    if (!string.IsNullOrEmpty(infopage.ImageUrl))
                    {
                        var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                        var pathString = Path.Combine(originalDirectory, infopage.ImageUrl);
                        if (System.IO.File.Exists(pathString))
                            System.IO.File.Delete(pathString);
                    }
                    LocalizationService.Delete(id);
                    db.InfoPages.Remove(infopage);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return HttpNotFound();
            }
        }
    }
}
