﻿using System;
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
        public ActionResult SetDefaultImage(string sellerId)
        {
            using (var db = new ApplicationDbContext())
            {
                var productsQuery = db.Products.Include(enty => enty.DefaultImage).Include(enty => enty.Images)
                .Where(entry => entry.IsActive && entry.Category.IsActive && entry.Seller.IsActive && entry.Images.Any() && entry.DefaultImageId == null);
                if(sellerId != null)
                {
                    productsQuery = productsQuery.Where(entry => entry.SellerId == sellerId);
                }
                var products = productsQuery.ToList();
                foreach (var product in products)
                {
                    var first = product.Images.OrderBy(entry => entry.Order).First();

                    var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                    var pathString = Path.Combine(originalDirectory, "Images", ImageType.ProductGallery.ToString(), product.Id);
                    if (first.IsAbsoluteUrl)
                    {
                        product.DefaultImageId = first.Id;
                    }
                    else if (System.IO.File.Exists(Path.Combine(pathString, first.ImageUrl)))
                    {
                        ImagesService imagesService = new ImagesService();
                        var format = imagesService.GetImageFormatByExtension(first.ImageUrl);
                        product.DefaultImageId = imagesService.AddProductDefaultImage(first, format);
                        db.Entry(product).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
            }
            return Content("Ok");
        }

        public ActionResult CleanImages()
        {
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var originalPath = Path.Combine(originalDirectory, "Images", "ProductGallery");
            var originalDir = new DirectoryInfo(originalPath);
            using (var db = new ApplicationDbContext())
            {
                foreach (var dir in originalDir.GetDirectories())
                {
                    var product = db.Products.Include(entry => entry.Images).FirstOrDefault(entry => entry.Id == dir.Name);
                    if (product == null || !dir.EnumerateFileSystemInfos().Any())
                    {
                        dir.Delete(true);
                    }
                    else
                    {
                        foreach (var img in dir.GetFiles())
                        {
                            if (product.Images.FirstOrDefault(entry => entry.ImageUrl == img.Name) == null)
                            {
                                System.IO.File.Delete(Path.Combine(dir.FullName, img.Name));
                            }
                        }
                    }
                }
            }
            return Content("Images cleaned");
        }

        public ActionResult AddCategoriesToOrderProducts(string sellerId)
        {
            using (var db = new ApplicationDbContext())
            {
                var orderProducts = db.Orders.Include(entry => entry.OrderProducts)
                .Where(entry => entry.SellerId == sellerId)
                .SelectMany(entry => entry.OrderProducts)
                .ToList();
                var productIds = orderProducts.Select(entry => entry.ProductId).ToList();
                var products = db.Products.AsNoTracking()
                    .Include(entry => entry.Category.MappedParentCategory)
                    .Where(entry => productIds.Contains(entry.Id)).ToList();
                foreach (var orderProduct in orderProducts)
                {
                    var product = products.FirstOrDefault(entry => entry.Id == orderProduct.ProductId);
                    if (product != null)
                    {
                        if (product.Category.MappedParentCategoryId == null)
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
            }
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
            using (var db = new ApplicationDbContext())
            {
                var productservice = new ProductsService();
                var products = db.Products.Where(entry => entry.SellerId == sellerId).Select(entry => entry.Id).ToList();
                foreach (var product in products)
                {
                    productservice.Delete(product);
                }
            }
            return Content("Ok");
        }
        public ActionResult Chat()
        {
            return PartialView();
        }
    }
}