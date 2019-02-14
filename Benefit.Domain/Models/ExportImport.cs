using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum SyncType
    {
        OneCCommerceMl,
        Yml,
        Excel,
        YmlExport
    }
    public class ExportImport
    {
        public ExportImport()
        {
            ExportProducts = new List<ExportProduct>();
        }
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        public bool IsImport { get; set; }
        public bool IsActive { get; set; }
        public SyncType SyncType { get; set; }
        [MaxLength(400)]
        public string FileUrl { get; set; }
        //in hours
        public int SyncPeriod { get; set; }
        public bool? LastUpdateStatus { get; set; }
        public string LastUpdateMessage { get; set; }
        public DateTime? LastSync { get; set; }
        public int? ProductsAdded { get; set; }
        public int? ProductsModified { get; set; }
        public int? ProductsRemoved { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        public bool HasNewContent { get; set; }

        public ICollection<ExportProduct> ExportProducts { get; set; }
    }
}
