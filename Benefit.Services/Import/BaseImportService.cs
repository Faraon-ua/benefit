using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Xml.Linq;

namespace Benefit.Services.Import
{
    public abstract class BaseImportService
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        protected string originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);

        protected abstract void ProcessImport(ExportImport importTask, ApplicationDbContext db);
        public void Import(string importTaskId)
        {
            using (var db = new ApplicationDbContext())
            {
                var importTask = db.ExportImports
                    .Include(entry => entry.Seller)
                    .Include(entry => entry.Seller.SellerCategories)
                    .FirstOrDefault(entry => entry.Id == importTaskId);
                if (importTask.IsImport == true) return;
                //show that import task is processing
                importTask.IsImport = true;
                db.SaveChanges();
                _logger.Info(string.Format("{2} Import started at {0} {1}", importTask.Seller.Id, importTask.Seller.UrlName, importTask.SyncType.ToString()));

                try
                {
                    ProcessImport(importTask, db);
                    importTask.LastUpdateStatus = true;
                    importTask.LastUpdateMessage = string.Format("{0} імпорт успішно проведено", importTask.SyncType.ToString());
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    importTask.LastUpdateStatus = false;
                    importTask.LastUpdateMessage = "Неможливо обробити файл "+ ex.ToString();
                }
                finally
                {
                    importTask.IsImport = false;
                    importTask.LastSync = DateTime.UtcNow;
                    db.Entry(importTask).State = EntityState.Modified;
                    db.SaveChanges();
                    _logger.Info(string.Format("{2} results saved {0} {1}", importTask.Seller.Id, importTask.Seller.Name, importTask.SyncType.ToString()));
                }
            }
        }

        public void DeleteImportCategories(Seller seller, IEnumerable<XElement> xmlCategories, SyncType importType, ApplicationDbContext db)
        {
            var currentSellercategoyIds = seller.MappedCategories.Select(entry => entry.ExternalIds).Distinct().ToList();
            List<string> xmlCategoryIds = null;
            if (importType == SyncType.OneCCommerceMl)
            {
                xmlCategoryIds = xmlCategories.Select(entry => entry.Element("Ид").Value).ToList();
            }
            if (importType == SyncType.Yml)
            {
                xmlCategoryIds = xmlCategories.Select(entry => entry.Attribute("id").Value).ToList();
            }
            if (importType == SyncType.Gbs)
            {
                xmlCategoryIds = xmlCategories.Select(entry => entry.Element("Id").Value).ToList();
            } 
            var catIdsToRemove = currentSellercategoyIds.Except(xmlCategoryIds).ToList();
            foreach (var catId in catIdsToRemove)
            {
                var dbCategories = db.Categories.Where(entry => entry.SellerId == seller.Id && entry.ExternalIds == catId).ToList();
                dbCategories.ForEach(entry =>
                {
                    entry.IsActive = false;
                    db.Entry(entry).State = EntityState.Modified;
                });
            }
        }
    }
}
