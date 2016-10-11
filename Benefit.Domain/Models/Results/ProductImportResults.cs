using System.Collections.Generic;

namespace Benefit.Web.Models.Admin
{
    public class ProductImportResults
    {
        public List<string> ProcessedCategories { get; set; }
        public List<string> IgnoredCategories { get; set; }
        public int ProductsAdded { get; set; }
        public int ProductsUpdated { get; set; }
        public int ProductsRemoved { get; set; }
    }
}