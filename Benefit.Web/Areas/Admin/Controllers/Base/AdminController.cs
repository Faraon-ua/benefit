using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Web.Models;

namespace Benefit.Web.Areas.Admin.Controllers.Base
{
    public class AdminController : Controller
    {
        public ActionResult SaveImagesOrder(List<string> sortedImages)
        {
            using (var db = new ApplicationDbContext())
            {
                var images = db.Images.Where(entry => sortedImages.Contains(entry.Id)).ToList();
                for (var i = 0; i < sortedImages.Count; i++)
                {
                    var img = images.FirstOrDefault(entry => entry.Id == sortedImages[i]);
                    img.Order = i;
                    db.Entry(img).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
            return Json("Сортування зображень збережено");
        }
        public ActionResult DeleteUploadedFile(string fileName, string parentId, ImageType type)
        {
            var imagesService = new ImagesService();
            if (Uri.IsWellFormedUriString(fileName, UriKind.Absolute))
            {
                imagesService.DeleteAbsoluteUrlImage(fileName, parentId);   
            }
            else
            {
                var dotIndex = fileName.LastIndexOf('.');
                var id = fileName.Substring(0, dotIndex);
                imagesService.Delete(id, parentId, type);
            }
            return Json(true);
        }

        public ActionResult SaveUploadedFile(ImageType type, string parentId)
        {
            var isSavedSuccessfully = true;
            var imageId = Guid.NewGuid().ToString();
            string fileExt = null;

            try
            {
                foreach (string fileName in Request.Files)
                {
                    var file = Request.Files[fileName];
                    if (file != null && file.ContentLength > 0)
                    {
                        var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                        var pathString = Path.Combine(originalDirectory, "Images", type.ToString(), parentId);
                        var dotIndex = file.FileName.LastIndexOf('.');
                        fileExt = file.FileName.Substring(dotIndex + 1);
                        var isExists = Directory.Exists(pathString);
                        if (!isExists)
                            Directory.CreateDirectory(pathString);
                        var path = string.Format("{0}\\{1}.{2}", pathString, imageId, fileExt);
                        file.SaveAs(path);
                        var imagesService = new ImagesService();
                        imagesService.ResizeToSiteRatio(path, ImageType.SellerGallery);
                    }
                }
            }
            catch (Exception ex)
            {
                isSavedSuccessfully = false;
            }
            if (isSavedSuccessfully)
            {
                using (var db = new ApplicationDbContext())
                {
                    switch (type)
                    {
                        case ImageType.SellerGallery:
                            db.Images.Add(new Image()
                            {
                                Id = imageId,
                                ImageType = type,
                                ImageUrl = string.Format("{0}.{1}", imageId, fileExt),
                                SellerId = parentId
                            });
                            break;
                        case ImageType.SellerSlider:
                            db.Images.Add(new Image()
                            {
                                Id = imageId,
                                ImageType = type,
                                ImageUrl = string.Format("{0}.{1}", imageId, fileExt),
                                SellerId = parentId
                            });
                            break;
                        case ImageType.ProductGallery:
                            db.Images.Add(new Image()
                            {
                                Id = imageId,
                                ImageType = type,
                                ImageUrl = string.Format("{0}.{1}", imageId, fileExt),
                                ProductId = parentId
                            });
                            break;
                    }
                    db.SaveChanges();
                }
                return Json(new { Message = string.Format("{0}.{1}", imageId, fileExt) });
            }
            return Json(new { Message = "Error in saving file" });
        }

        public ActionResult CategoryForm(int number, string categoryId, string sellerId)
        {
            using (var db = new ApplicationDbContext())
            {
                ViewBag.Categories =
                    db.Categories.Where(entry => !entry.IsSellerCategory).ToList().Select(
                        entry =>
                            new HierarchySelectItem()
                            {
                                Text = entry.Name,
                                Value = entry.Id,
                                Selected = entry.Id == categoryId
                            });
                var sellerCategory =
                    db.SellerCategories.FirstOrDefault(
                        entry => entry.CategoryId == categoryId && entry.SellerId == sellerId) ?? new SellerCategory()
                        {
                            SellerId = sellerId,
                            CategoryId = categoryId
                        };

                return PartialView("_CategoryForm",
                       new KeyValuePair<int, SellerCategory>(number, sellerCategory));
            }
        }
        public ActionResult NewCurrencyForm(int number)
        {
            return PartialView("_CurrencyForm",
                new KeyValuePair<int, Currency>(number, new Currency() { Id = Guid.NewGuid().ToString() }));
        }
        public ActionResult NewAddressForm(int number)
        {
            return PartialView("_AddressForm",
                new KeyValuePair<int, Address>(number, new Address()));
        }
        public ActionResult NewShippingForm(int number)
        {
            return PartialView("_ShippingForm",
                new KeyValuePair<int, ShippingMethod>(number, new ShippingMethod() { Id = Guid.NewGuid().ToString() }));
        }

    }
}