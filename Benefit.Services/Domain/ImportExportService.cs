using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.Services.Domain
{
    public class ImportExportService
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private CategoriesService categoriesService = new CategoriesService();

        private void CreateAndUpdatePromUaCategories(List<XElement> xmlCategories, string sellerUrlName, Category parent = null)
        {
            List<XElement> xmlCats = null;
            if (parent == null)
            {
                xmlCats = xmlCategories.Where(entry => entry.Attribute("parentId") == null).ToList();
            }
            else
            {
                xmlCats = xmlCategories.Where(entry => entry.Attribute("parentId") != null && entry.Attribute("parentId").Value == parent.Id).ToList();
            }
            foreach (var xmlCategory in xmlCats)
            {
                var catId = xmlCategory.Attribute("id").Value;
                var catName = xmlCategory.Value.Replace("\n", "").Replace("\r", "");
                var dbCategory = db.Categories.FirstOrDefault(entry => entry.Id == catId);
                if (dbCategory == null)
                {
                    dbCategory = new Category()
                    {
                        Id = catId,
                        ParentCategoryId = xmlCategory.Attribute("parentId") == null ? null : xmlCategory.Attribute("parentId").Value,
                        IsSellerCategory = true,
                        Name = catName,
                        UrlName = string.Format("{0}_{1}_{2}", sellerUrlName, parent == null ? string.Empty : parent.Name.Translit(), catName.Translit()),
                        Description = catName,
                        NavigationType = CategoryNavigationType.SellersAndProducts.ToString(),
                        IsActive = true,
                        LastModified = DateTime.UtcNow,
                        LastModifiedBy = "ImportFromPromua"
                    };
                    db.Categories.Add(dbCategory);
                }
                else
                {
                    dbCategory.Name = xmlCategory.Value;
                    dbCategory.UrlName = string.Format("{0}_{1}_{2}", sellerUrlName,
                        parent == null ? string.Empty : parent.Name.Translit(), catName.Translit());

                    dbCategory.ParentCategoryId = xmlCategory.Attribute("parentId") == null
                        ? null
                        : xmlCategory.Attribute("parentId").Value;
                    db.Entry(dbCategory).State = EntityState.Modified;
                }

                CreateAndUpdatePromUaCategories(xmlCategories, sellerUrlName, dbCategory);
            }
        }

        private void DeletePromUaCategories(Seller seller, IEnumerable<XElement> xmlCategories, bool delete)
        {
            var currentSellercategoyIds = seller.MappedCategories.Select(entry => entry.Id).ToList();
            var xmlCategoryIds = xmlCategories.Select(entry => entry.Attribute("id").Value).ToList();
            var catIdsToRemove = currentSellercategoyIds.Except(xmlCategoryIds).ToList();
            foreach (var catId in catIdsToRemove)
            {
                 if (delete)
            {
                categoriesService.Delete(catId);
            }
            else
                 {
                     var dbCategory = db.Categories.Find(catId);
                     dbCategory.IsActive = false;
                     db.Entry(dbCategory).State = EntityState.Modified;
                 }
            }
        }

        private void AddAndUpdatePromUaProducts(List<XElement> xmlProducts)
        {
            foreach (var xmlProduct in xmlProducts)
            {
                var product = db.Products.FirstOrDefault(entry => entry.Id == xmlProduct.Attribute("id").Value);
                if (product == null)
                {
                    var name = xmlProduct.Element("name").Value;
                    var descr = xmlProduct.Element("description").Value;
                    if (xmlProduct.Element("vendor") != null || xmlProduct.Elements("param").Any())
                    {
                        
                    }
                    product = new Product()
                    {
                        Id = xmlProduct.Attribute("id").Value,
                        Name = name,
                        UrlName = name.Translit(),
                        CategoryId = xmlProduct.Element("categoryId").Value,
                        SKU = db.Products.Max(entry=>entry.SKU) +1,
                        Description = 
                    }
                }
            }
        }


        public void ImportFromPromua(string sellerId)
        {
            {
                var importTasks =
                    db.ExportImports.Include(entry => entry.Seller.MappedCategories).Where(
                        entry => entry.IsActive && entry.IsImport && entry.SyncType == SyncType.Promua).ToList();
                foreach (var importTask in importTasks)
                {
                    XDocument xml = null;
                    try
                    {
                        xml = XDocument.Load(importTask.FileUrl);
                    }
                    catch (Exception ex)
                    {
                        importTask.LastUpdateStatus = false;
                        importTask.LastUpdateMessage = "Неможливо обробити файл, перевірте правильність посилання на файл";
                        importTask.LastSync = DateTime.UtcNow;
                        db.Entry(importTask).State = EntityState.Modified;
                        db.SaveChanges();
                        return;
                    }

                    var root = xml.Element("yml_catalog").Element("shop");
                    var xmlCategories = root.Descendants("categories").First().Elements().ToList();
                    CreateAndUpdatePromUaCategories(xmlCategories, importTask.Seller.UrlName);
                    db.SaveChanges();
                    DeletePromUaCategories(importTask.Seller, xmlCategories, importTask.RemoveProducts);
                    db.SaveChanges();

                    var xmlProducts = root.Descendants("offers").First().Elements().ToList();
                    AddAndUpdatePromUaProducts(xmlProducts);
                    db.SaveChanges();

                    importTask.LastUpdateStatus = true;
                    importTask.LastUpdateMessage = null;
                    importTask.LastSync = DateTime.UtcNow;
                    db.Entry(importTask).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }
    }
}
