using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Image = System.Drawing.Image;

namespace Benefit.Services
{
    public class ImagesService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ImageFormat GetImageFormatByExtension(string imagePath)
        {
            var extension = Path.GetExtension(imagePath);
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException(
                    string.Format("Unable to determine file extension for fileName: {0}", imagePath));

            switch (extension.ToLower())
            {
                case @".bmp":
                    return ImageFormat.Bmp;

                case @".gif":
                    return ImageFormat.Gif;

                case @".ico":
                    return ImageFormat.Icon;

                case @".jpg":
                case @".jpeg":
                    return ImageFormat.Jpeg;

                case @".png":
                    return ImageFormat.Png;

                case @".tif":
                case @".tiff":
                    return ImageFormat.Tiff;

                case @".wmf":
                    return ImageFormat.Wmf;

                default:
                    throw new NotImplementedException();
            }
        }

        public void AddImage(string entityId, string fileName, ImageType type)
        {
            var image = new Benefit.Domain.Models.Image()
            {
                Id = Guid.NewGuid().ToString(),
                ImageType = type,
                ImageUrl = fileName
            };
            if (type == ImageType.SellerGallery || type == ImageType.SellerGallery)
            {
                image.SellerId = entityId;
            }
            if (type == ImageType.ProductGallery)
            {
                image.ProductId = entityId;
            }
            db.Images.Add(image);
            db.SaveChanges();
        }

        public void ResizeToSiteRatio(string imagePath, ImageType imageType)
        {
            var img = Image.FromFile(imagePath);
            int maxHeight = 0;
            int maxWidth = 0;
            switch (imageType)
            {
                case ImageType.SellerLogo:
                    maxWidth = SettingsService.Images.SellerLogoMaxWidth;
                    maxHeight = SettingsService.Images.SellerLogoMaxHeight;
                    break;
                case ImageType.NewsLogo:
                    maxWidth = SettingsService.Images.NewsLogoMaxWidth;
                    maxHeight = SettingsService.Images.NewsLogoMaxHeight;
                    break;
                case ImageType.SellerGallery:
                    maxHeight = SettingsService.Images.SellerGalleryImageMaxHeight;
                    maxWidth = SettingsService.Images.SellerGalleryImageMaxWidth;
                    break; 
                case ImageType.UserAvatar:
                    maxHeight = SettingsService.Images.SellerGalleryImageMaxHeight;
                    maxWidth = SettingsService.Images.SellerGalleryImageMaxWidth;
                    break;
            }
            if (img.Height <= maxHeight &&
                img.Width <= maxWidth) return;
            Bitmap newImage;
            using (img)
            {
                Double xRatio = (double)img.Width / maxWidth;
                Double yRatio = (double)img.Height / maxHeight;
                Double ratio = Math.Max(xRatio, yRatio);
                var nnx = (int)Math.Floor(img.Width / ratio);
                var nny = (int)Math.Floor(img.Height / ratio);
                newImage = new Bitmap(nnx, nny, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(newImage))
                {
                    gr.Clear(Color.Transparent);
                    // This is said to give best quality when resizing images
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.DrawImage(img,
                        new Rectangle(0, 0, nnx, nny),
                        new Rectangle(0, 0, img.Width, img.Height),
                        GraphicsUnit.Pixel);
                }
            }
            using (var memory = new MemoryStream())
            {
                using (var fs = new FileStream(imagePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    newImage.Save(memory, GetImageFormatByExtension(imagePath));
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }

        public void DeleteAll(IEnumerable<Benefit.Domain.Models.Image> images, string parentId, ImageType type)
        {
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var pathString = Path.Combine(originalDirectory, "Images", type.ToString(), parentId);
            var productImagesDir = new DirectoryInfo(pathString);
            if (productImagesDir.Exists)
            {
                productImagesDir.Delete(true);
            }
            using (var db = new ApplicationDbContext())
            {
                var imgIds = images.Select(img => img.Id).ToList();
                db.Images.RemoveRange(db.Images.Where(entry => imgIds.Contains(entry.Id)));
            }
        }

        public void Delete(string imageId, string parentId, ImageType type)
        {
            var image = db.Images.Find(imageId);
            if (image == null) return;
            DeleteFile(image.ImageUrl, parentId, type);
        }

        public void DeleteFile(string fileName, string parentId, ImageType type)
        {
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var pathString = Path.Combine(originalDirectory, "Images", type.ToString());
            var fullPath = Path.Combine(pathString, parentId, fileName);
            try
            {
                var file = new FileInfo(fullPath);
                if (file.Exists)
                {
                    File.SetAttributes(fullPath, FileAttributes.Normal);
                    file.Delete();
                }
            }
            catch(Exception e)
            {
                Debug.Write(e.Message);
            }
            var dotIndex = fileName.IndexOf('.');
            var id = fileName.Substring(0, dotIndex);
            using (var db = new ApplicationDbContext())
            {
                var image = db.Images.Find(id);
                if (image != null)
                {
                    db.Images.Remove(image);
                    db.SaveChanges();
                }
            }
        }
    }
}
