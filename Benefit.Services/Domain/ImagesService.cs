using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
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

        public void Delete(string imageId, ImageType type)
        {
            var image = db.Images.Find(imageId);
            if(image ==null) return;
            DeleteFile(image.ImageUrl, type);
        }

        private void DeleteFile(string fileName, ImageType type)
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
        }
    }
}
