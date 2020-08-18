using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Services.Domain;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Controllers
{
    public class HelperController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult CleanImages()
        {
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var originalPath = Path.Combine(originalDirectory, "Images", "ProductGallery");
            var originalDir = new DirectoryInfo(originalPath);
            foreach (var dir in originalDir.GetDirectories())
            {
                var product = db.Products.Include(entry => entry.Images).FirstOrDefault(entry => entry.Id == dir.Name);
                if(product == null || !dir.EnumerateFileSystemInfos().Any())
                {
                    dir.Delete(true);
                }
                else
                {
                    foreach(var img in dir.GetFiles())
                    {
                        if(product.Images.FirstOrDefault(entry=>entry.ImageUrl == img.Name) == null)
                        {
                            System.IO.File.Delete(Path.Combine(dir.FullName, img.Name));
                        }
                    }
                }
            }
            return Content("Images cleaned");
        }

        public ActionResult AddCategoriesToOrderProducts(string sellerId)
        {
            var orderProducts = db.Orders.Include(entry => entry.OrderProducts)
                .Where(entry => entry.SellerId == sellerId)
                .SelectMany(entry => entry.OrderProducts)
                .ToList();
            var productIds = orderProducts.Select(entry => entry.ProductId).ToList();
            var products = db.Products.AsNoTracking()
                .Include(entry => entry.Category.MappedParentCategory)
                .Where(entry => productIds.Contains(entry.Id)).ToList();
            foreach(var orderProduct in orderProducts)
            {
                var product = products.FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                if (product != null)
                {
                    if(product.Category.MappedParentCategoryId== null)
                    {
                        orderProduct.CategoryName = product.Category.Name;
                    }
                    else
                    {
                        orderProduct.CategoryName = product.Category.MappedParentCategory.Name;
                    }
                    db.Entry(orderProduct).State = EntityState.Modified;
                }
            }
            db.SaveChanges();
            return Content("OK");
        }
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