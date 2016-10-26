using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Benefit.Common.Extensions;
using Benefit.DataTransfer;
using Benefit.Domain.DataAccess;
using System.Data.Entity;
using Benefit.Domain.Models;
using Benefit.Domain.Models.ModelExtensions;
using Benefit.Domain.Models.XmlModels;

namespace Benefit.Services.Domain
{
    public class CategoriesService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public List<Category> GetBreadcrumbs(string categoryId)
        {
            var resultList = new List<Category>();
            var category = db.Categories.Include(entry => entry.ParentCategory).FirstOrDefault(entry => entry.Id == categoryId);
            resultList.Add(category);
            while (category.ParentCategory != null)
            {
                resultList.Add(category.ParentCategory);
                category = category.ParentCategory;
            }
            resultList.Reverse();
            return resultList;
        }

        public SellersDto GetCategorySellers(string urlName)
        {
            var sellerIds = new List<string>();
            var category = db.Categories.Include(entry => entry.ChildCategories).Include(entry => entry.SellerCategories).FirstOrDefault(entry => entry.UrlName == urlName);
            if (category == null) return null;
            var sellersDto = new SellersDto()
            {
                Category = category
            };
            sellerIds.AddRange(category.SellerCategories.Select(entry => entry.SellerId));
            sellerIds.AddRange(category.GetAllChildrenRecursively().SelectMany(entry => entry.SellerCategories).Select(entry => entry.SellerId));

            var regionId = RegionService.GetRegionId();
            sellersDto.Items =
                db.Sellers
                .Include(entry => entry.Images)
                .Include(entry => entry.Addresses)
                .Include(entry => entry.ShippingMethods.Select(sm => sm.Region))
                .Include(entry => entry.SellerCategories.Select(sc => sc.Category.ParentCategory))
                .Where(
                    entry =>
                        sellerIds.Contains(entry.Id) &&
                        entry.IsActive &&
                        entry.Addresses.Select(addr => addr.RegionId).Contains(regionId)).ToList();
            sellersDto.Items.ForEach(entry =>
            {
                entry.Addresses = entry.Addresses.Where(addr => addr.RegionId == regionId).ToList();
                entry.ShippingMethods =
                    entry.ShippingMethods.Where(sh => sh.RegionId == entry.ShippingMethods.Min(shm => shm.RegionId))
                        .ToList();
            });
            sellersDto.Breadcrumbs = GetBreadcrumbs(category.Id);

            return sellersDto;
        }

        public void AddOrUpdateCategoriesFromXml(Seller seller, IEnumerable<XmlCategory> xmlCategories)
        {
            var defaultCategory = seller.SellerCategories.First(entry => entry.IsDefault).Category;
            var allDbCategories = defaultCategory.GetAllChildrenRecursively().ToList();

            var xmlToDbCategoriesMapping = new Dictionary<string, string>();
            foreach (var dbCategory in allDbCategories)
            {
                var xmlCategory = xmlCategories.FirstOrDefault(entry => entry.Name == dbCategory.Name);
                if (xmlCategory != null)
                {
                    xmlToDbCategoriesMapping.Add(xmlCategory.Id, dbCategory.Id);
                }
            }
        }

        private void SaveCategoryFromXmlCategory(XmlCategory xmlCategory, IEnumerable<XmlCategory> xmlCategories)
        {
            var existingCategory = db.Categories.Find(xmlCategory.Id);
            if (existingCategory != null)
            {
                existingCategory.Name = xmlCategory.Name;
                existingCategory.ParentCategoryId = xmlCategory.ParentId;
                db.Entry(existingCategory).State = EntityState.Modified;
                existingCategory.LastModified = DateTime.UtcNow;
                existingCategory.LastModifiedBy = "1CFileImport";
            }
            else
            {
                db.Categories.Add(new Category()
                {
                    Id = xmlCategory.Id,
                    Name = xmlCategory.Name,
                    UrlName = xmlCategory.Name.Translit(),
                    ParentCategoryId = xmlCategory.ParentId,
                    IsActive = true,
                    NavigationType = CategoryNavigationType.SellersAndProducts.ToString(),
                    LastModified = DateTime.UtcNow,
                    LastModifiedBy = "1CFileImport"
                });
            }

            var childCategories = xmlCategories.Where(entry => entry.ParentId == xmlCategory.Id).ToList();
            if (xmlCategories.Any())
            {
                childCategories.ForEach(entry => SaveCategoryFromXmlCategory(entry, xmlCategories));
            }
        }

        public void Delete(string id)
        {
            var category = db.Categories.Include("ChildCategories").FirstOrDefault(entry => entry.Id == id);
            if (category == null) return;
            foreach (var childCategory in category.ChildCategories)
            {
                Delete(childCategory.Id);
            }

            category.Products.ToList().ForEach(entry =>
            {
                entry.CategoryId = null;
                db.Entry(entry).State = EntityState.Modified;
            });
            db.ProductParameters.RemoveRange(category.ProductParameters);
            db.Localizations.RemoveRange(db.Localizations.Where(entry => entry.ResourceId == category.Id));
            if (category.ImageUrl != null)
            {
                var image = new FileInfo(Path.Combine(HttpContext.Current.Server.MapPath("~/Images/"), category.ImageUrl));
                if (image.Exists)
                    image.Delete();
            }
            db.Categories.Remove(category);
            db.SaveChanges();
        }
    }
}
