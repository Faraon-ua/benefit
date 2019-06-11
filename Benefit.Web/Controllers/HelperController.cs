using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Web.Filters;
using Benefit.Web.Helpers;
using Benefit.Services.Domain;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Controllers
{
    public class HelperController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult ResizeAllImages(ImageType imageType)
        {
            var imagesService = new ImagesService();
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var pathString = Path.Combine(originalDirectory, "Images", imageType.ToString());
            var dir = new DirectoryInfo(pathString);
            foreach (var img in dir.GetFiles())
            {
                imagesService.ResizeToSiteRatio(Path.Combine(pathString, img.Name), imageType);
            }
            return Content("Ok");
        }

        public ActionResult DeleteAllSellerProducts(string sellerId)
        {
            var productservice = new ProductsService();
            var products = db.Products.Where(entry => entry.SellerId == sellerId).Select(entry=>entry.Id).ToList();
            foreach(var product in products)
            {
                productservice.Delete(product);
            }
            return Content("Ok");
        }
        public ActionResult Chat()
        {
            return PartialView();
        }

        public ActionResult Comments()
        {
            var allComments = db.Messages.Include(entry => entry.User).OrderByDescending(entry => entry.TimeStamp).ToList();
            return PartialView(allComments);
        }

        public ActionResult DeleteComment(string messageId)
        {
            var message = db.Messages.Find(messageId);
            db.Messages.Remove(message);
            db.SaveChanges();
            return Content("success");
        }

        public ActionResult AddChatMessage(string message)
        {
            var userId = User.Identity.GetUserId();
            var chatMessage = new Message()
            {
                Id = Guid.NewGuid().ToString(),
                Text = message,
                TimeStamp = DateTime.UtcNow,
                UserId = userId
            };
            db.Messages.Add(chatMessage);
            db.SaveChanges();
            var allComments = db.Messages.Include(entry => entry.User).OrderByDescending(entry => entry.TimeStamp).ToList();
            return PartialView("Comments", allComments);
        }
    }
}