using System;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum SyncType
    {
        OneCCommerceMl,
        Promua,
        Excel
    }
    public class ExportImport
    {
        public string Id { get; set; }
        public bool IsImport { get; set; }
        public bool IsActive { get; set; }
        public SyncType SyncType { get; set; }
        public string FileUrl { get; set; }
        //in hours
        public int SyncPeriod { get; set; }
        public bool? LastUpdateStatus { get; set; }
        public string LastUpdateMessage { get; set; }
        public DateTime? LastSync { get; set; }
        public int? ProductsAdded { get; set; }
        public int? ProductsModified { get; set; }
        public int? ProductsRemoved { get; set; }
        [Required]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        public bool HasNewContent { get; set; }
    }
}
