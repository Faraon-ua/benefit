using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.XmlModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Benefit.DataTransfer.Results;
using System.IO;

namespace Benefit.Services.Domain
{
    public class BannersService
    {
        public void Delete(string bannerId)
        {
            using (var db = new ApplicationDbContext())
            {
                var banner = db.Banners.Find(bannerId);
                Delete(banner, db);
            }
        }

        public void Delete(Banner banner, ApplicationDbContext db)
        {
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var pathString = Path.Combine(originalDirectory, "Images", banner.BannerType.ToString(), banner.ImageUrl);
            var file = new FileInfo(pathString);
            if (file.Exists)
            {
                file.Delete();
            }
            db.Banners.Remove(banner);
            db.SaveChanges();
        }
    }
}

