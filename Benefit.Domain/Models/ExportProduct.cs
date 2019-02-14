using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class ExportProduct
    {
        [Key, Column(Order = 0)]
        public string ProductId { get; set; }
        public Product Product { get; set; }
        [Key, Column(Order = 1)]
        public string ExportId { get; set; }
        public ExportImport Export { get; set; }
    }
}
