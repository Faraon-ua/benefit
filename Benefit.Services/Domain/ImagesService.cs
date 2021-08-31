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
using ImageProcessor;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using Image = Benefit.Domain.Models.Image;

namespace Benefit.Services
{
    public class ImagesService
    {
        public string AddProductDefaultImage(Image img, ImageFormat format)
        {
            using (var db = new ApplicationDbContext())
            {
                if (img.IsAbsoluteUrl)
                    return img.Id;
                var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                var pathString = Path.Combine(originalDirectory, "Images", ImageType.ProductGallery.ToString(), img.ProductId);
                var path = Path.Combine(pathString, img.ImageUrl);
                if (!File.Exists(path))
                    return img.Id;
                var imgId = Guid.NewGuid().ToString();
                var defaultImgName = imgId + ".webp";
                File.Copy(Path.Combine(pathString, img.ImageUrl), Path.Combine(pathString, defaultImgName));
                var image = new Image()
                {
                    Id = imgId,
                    ImageUrl = defaultImgName,
                    IsAbsoluteUrl = false,
                    ProductId = img.ProductId,
                    ImageType = ImageType.ProductDefault
                };
                db.Images.Add(image);
                db.SaveChanges();
                ResizeToSiteRatio(Path.Combine(pathString, defaultImgName), ImageType.ProductDefault, imageFormat: format);
                return imgId;
            }
        }
        public ImageFormat GetImageFormatByExtension(string imagePath)
        {
            var extension = Path.GetExtension(imagePath);
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException(
                    string.Format("Unable to determine file extension for fileName: {0}", imagePath));

            switch (extension.Trim().ToLower())
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
                    return null;
            }
        }

        public void AddImage(string entityId, string fileName, ImageType type, int order = 0)
        {
            using (var db = new ApplicationDbContext())
            {
                var image = new Image()
                {
                    Id = Guid.NewGuid().ToString(),
                    ImageType = type,
                    ImageUrl = fileName,
                    Order = order
                };
                if (type == ImageType.SellerGallery || type == ImageType.SellerGallery || type == ImageType.SellerCatalog)
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
        }

        public void ResizeToSiteRatio(string imagePath, ImageType imageType, BannerType? bannerType = null, ImageFormat imageFormat = null, SellerEcommerceTemplate? sellerTemplate = null)
        {
            if (!File.Exists(imagePath)) return;
            Bitmap img;
            using (var bmpTemp = new Bitmap(imagePath))
            {
                img = new Bitmap(bmpTemp);
            }
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
                case ImageType.CategoryLogo:
                    maxHeight = SettingsService.Images.CategoryLogoMaxHeight;
                    maxWidth = SettingsService.Images.CategoryLogoMaxWidth;
                    break;
                case ImageType.ProductGallery:
                    maxWidth = SettingsService.Images.ProductGalleryMaxWidth;
                    maxHeight = SettingsService.Images.ProductGalleryMaxHeight;
                    break;
                case ImageType.ProductDefault:
                    maxWidth = 290;
                    maxHeight = 238;
                    break;
                case ImageType.Banner:
                    if (bannerType == BannerType.FirstRowMainPage || bannerType == BannerType.FirstRowMainPage)
                    {
                        maxWidth = 1400;
                        maxHeight = 200;
                    }
                    if (bannerType == BannerType.PrimaryMainPage)
                    {
                        if (!sellerTemplate.HasValue)
                        {
                            maxWidth = 724;
                            maxHeight = 433;
                        }
                        else
                        {
                            if (sellerTemplate.Value == SellerEcommerceTemplate.MegaShop)
                            {
                                maxWidth = 994;
                                maxHeight = 487;
                            }
                            if (sellerTemplate.Value == SellerEcommerceTemplate.EcolifeFurniture)
                            {
                                maxWidth = 1520;
                                maxHeight = 550;
                            }
                        }
                    }
                    if (bannerType == BannerType.SideTopMainPage || bannerType == BannerType.SideBottomMainPage)
                    {
                        maxWidth = 377;
                        maxHeight = 212;
                    }
                    break;
            }
            if (img.Height <= maxHeight &&
                img.Width <= maxWidth)
            {
                if (imagePath.Contains(".webp"))
                {
                    ConvertToWebp(new Bitmap(img), imagePath, imageFormat);
                }
                return;
            }
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
            if (imagePath.Contains(".webp"))
            {
                ConvertToWebp(newImage, imagePath, imageFormat);
            }
            else
            {
                using (var memory = new MemoryStream())
                {
                    using (var fs = new FileStream(imagePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        var format = GetImageFormatByExtension(imagePath);
                        if (format == null) return;
                        newImage.SetResolution(300, 300);
                        newImage.Save(memory, format);
                        byte[] bytes = memory.ToArray();
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
            }
        }

        public void ConvertToWebp(Bitmap newImage, string imagePath, ImageFormat imageFormat)
        {
            using (var memory = new MemoryStream())
            {
                newImage.Save(memory, imageFormat);
                var bytes = memory.ToArray();
                using (var imageFactory = new ImageFactory(preserveExifData: false))
                {
                    imageFactory.Load(bytes)
                                .Format(new WebPFormat())
                                .Save(imagePath);
                }
            }
        }
        public void DeleteAll(IEnumerable<Image> images, string parentId, ImageType type, bool deleteFolder = false, bool deleteFromDb = true)
        {
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var pathString = Path.Combine(originalDirectory, "Images", type.ToString(), parentId == null ? string.Empty : parentId);
            var productImagesDir = new DirectoryInfo(pathString);
            if (productImagesDir.Exists)
            {
                if (deleteFolder)
                {
                    productImagesDir.Delete(true);
                }
                else
                {
                    foreach (var file in productImagesDir.GetFiles())
                    {
                        file.Delete();
                    }
                }
            }
            if (deleteFromDb)
            {
                using (var db = new ApplicationDbContext())
                {
                    var imgIds = images.Select(img => img.Id).ToList();
                    db.Images.RemoveRange(db.Images.Where(entry => imgIds.Contains(entry.Id)));
                    db.SaveChanges();
                }
            }
        }

        public void Delete(string imageId, string parentId, ImageType type)
        {
            using (var db = new ApplicationDbContext())
            {
                Image image = null;
                if (type == ImageType.ProductGallery)
                {
                    image = db.Images.FirstOrDefault(entry => entry.Id == imageId && entry.ProductId == parentId);
                }
                else
                {
                    image = db.Images.Find(imageId);
                }
                if (image == null) return;
                DeleteFile(image.ImageUrl, parentId, type);
            }
        }

        public void DeleteAbsoluteUrlImage(string url, string productId)
        {
            using (var db = new ApplicationDbContext())
            {
                var image = db.Images.FirstOrDefault(entry => entry.ImageUrl == url && entry.ProductId == productId);
                if (image == null) return;
                var defaultImgProduct = db.Products.FirstOrDefault(entry => entry.DefaultImageId == image.Id);
                if (defaultImgProduct != null)
                {
                    var otherImage = db.Images.FirstOrDefault(entry => entry.Id != image.Id && entry.ProductId == productId);
                    if (otherImage != null)
                    {
                        defaultImgProduct.DefaultImageId = otherImage.Id;
                    }
                    db.SaveChanges();
                }
                db.Images.Remove(image);
                db.SaveChanges();
            }
        }

        public void DeleteFile(string fileName, string parentId, ImageType type)
        {
            var dotIndex = fileName.IndexOf('.');
            var id = dotIndex > 0 ? fileName.Substring(0, dotIndex) : fileName;
            Image image = null;
            using (var db = new ApplicationDbContext())
            {
                if (type == ImageType.ProductGallery)
                {
                    image = db.Images.FirstOrDefault(entry => (entry.ImageUrl == fileName || entry.Id == id) && entry.ProductId == parentId);
                }
                else
                {
                    image = db.Images.FirstOrDefault(entry => entry.ImageUrl == fileName || entry.Id == id);
                }
                if (image == null) return;
                db.Images.Remove(image);
                db.SaveChanges();
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
                catch (Exception e)
                {
                    Debug.Write(e.Message);
                }
                
            }
        }

        public void DeleteWithFile(Image img, ApplicationDbContext db)
        {
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var pathString = Path.Combine(originalDirectory, "Images", img.ImageType.ToString());
            var fullPath = Path.Combine(pathString, img.ImageUrl);
            try
            {
                var file = new FileInfo(fullPath);
                if (file.Exists)
                {
                    File.SetAttributes(fullPath, FileAttributes.Normal);
                    file.Delete();
                }
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
            }
            db.Images.Remove(img);
            db.SaveChanges();
        }
    }
}
