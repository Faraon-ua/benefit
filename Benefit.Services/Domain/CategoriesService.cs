using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using System.Data.Entity;
using Benefit.Domain.ModelExtensions;
using Benefit.Domain.Models;
using Benefit.Domain.Models.XmlModels;

namespace Benefit.Services.Domain
{
    public class CategoriesService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

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

            /*var xmlCategoryIds = xmlCategories.Select(entry => entry.Id).ToList();
            //select all 
            var dbCategoryIds = seller.SellerCategories.Where(entry=>!entry.IsDefault).Select(entry => entry.CategoryId).ToList();

            //remove categories
            var categoryIdsToDelete = dbCategoryIds.Except(xmlCategoryIds).ToList();
            categoryIdsToDelete.ToList().ForEach(Delete);

            //add or update categories
            foreach (var xmlCategory in xmlCategories.Where(entry => !categoryIdsToDelete.Contains(entry.Id) && entry.ParentId == null))
            {
                SaveCategoryFromXmlCategory(xmlCategory, xmlCategories);
            }

            db.SaveChanges();*/
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
                childCategories.ForEach(entry=>SaveCategoryFromXmlCategory(entry, xmlCategories));
            }
        }

        public void Delete(string id)
        {
            var category = db.Categories.Include("ChildCategories").FirstOrDefault(entry=>entry.Id == id);
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
