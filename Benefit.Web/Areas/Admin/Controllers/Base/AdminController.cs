using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services;

namespace Benefit.Web.Areas.Admin.Controllers.Base
{
    [Authorize(Roles = "Admin,Seller")]
    public class AdminController : Controller
    {
        public ActionResult DeleteUploadedFile(string fileName, ImageType type)
        {
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var pathString = Path.Combine(originalDirectory, "Images", type.ToString());
            var file = new FileInfo(Path.Combine(pathString, fileName));
            file.Delete();
            var dotIndex = fileName.IndexOf('.');
            var id = fileName.Substring(0, dotIndex);
            using (var db = new ApplicationDbContext())
            {
                var image = db.Images.Find(id);
                db.Images.Remove(image);
                db.SaveChanges();
            }
            return Json(true);
        }

        public ActionResult SaveUploadedFile(ImageType type, string parentId)
        {
            bool isSavedSuccessfully = true;
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
                        var pathString = Path.Combine(originalDirectory, "Images", type.ToString());
                        var slashIndex = file.ContentType.IndexOf('/');
                        fileExt = file.ContentType.Substring(slashIndex + 1);
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

        public ActionResult NewCurrencyForm(int number)
        {
            return PartialView("_CurrencyForm",
                new KeyValuePair<int, Currency>(number, new Currency()));
        }
        public ActionResult NewAddressForm(int number)
        {
            return PartialView("_AddressForm",
                new KeyValuePair<int, Address>(number, new Address()));
        }
        public ActionResult NewShippingForm(int number)
        {
            return PartialView("_ShippingForm",
                new KeyValuePair<int, ShippingMethod>(number, new ShippingMethod()));
        }

    }
}