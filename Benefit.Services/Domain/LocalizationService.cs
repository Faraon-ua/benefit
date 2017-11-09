using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Files;

namespace Benefit.Services
{
    public class LocalizationService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public List<Localization> Get<T>(T obj, string[] fields)
        {
            var id = obj.GetType().GetProperty("Id").GetValue(obj, null).ToString();
            var type = ObjectContext.GetObjectType(obj.GetType()).ToString();
            var localizations = new List<Localization>();
            foreach (var supportedLocalization in SettingsService.SupportedLocalizations)
            {
                foreach (var field in fields)
                {
                    var localization =
                        db.Localizations.FirstOrDefault(
                            entry =>
                                entry.ResourceId == id && entry.ResourceField == field &&
                                entry.ResourceType == type)
                        ??
                        new Localization()
                        {
                            Id = Guid.NewGuid().ToString(),
                            LanguageCode = supportedLocalization,
                            ResourceField = field,
                            ResourceId = id,
                            ResourceType = obj.GetType().ToString()
                        };
                    localizations.Add(localization);
                }
            }
            return localizations;
        }

        public void Save(List<Localization> localizations)
        {
            var resourceId = localizations.First().ResourceId;
            var resourceType = localizations.First().ResourceType;
            var existingLocalizations = db.Localizations.Where(entry =>
                entry.ResourceId == resourceId &&
                entry.ResourceType == resourceType);
            db.Localizations.RemoveRange(existingLocalizations);
            db.SaveChanges();
            db.Localizations.AddRange(localizations);
            db.SaveChanges();
        }

        public byte[] ExportProducts(string sellerId)
        {
            var fileService = new FilesExportService();
            var products = db.Products.Where(entry => entry.SellerId == sellerId).ToList();
            var productIds = products.Select(pr => pr.Id).ToList();
            var localizations = db.Localizations.Where(entry => productIds.Contains(entry.ResourceId)).OrderBy(entry=>entry.ResourceId).ThenBy(entry=>entry.ResourceField).ToList();
            foreach (var localization in localizations)
            {
                localization.ResourceName = products.FirstOrDefault(entry => entry.Id == localization.ResourceId).Name;
            }
            return fileService.CreateCSVFromGenericList(localizations); 
        }

        public void Delete(string resourceId)
        {
            var localizations = db.Localizations.Where(entry => entry.ResourceId == resourceId);
            db.Localizations.RemoveRange(localizations);
            db.SaveChanges();
        }
    }
}