using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

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

        public void Delete(string resourceId)
        {
            var localizations = db.Localizations.Where(entry => entry.ResourceId == resourceId);
            db.Localizations.RemoveRange(localizations);
            db.SaveChanges();
        }
    }
}