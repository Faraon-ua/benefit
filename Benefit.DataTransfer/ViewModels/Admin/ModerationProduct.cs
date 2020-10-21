using Benefit.Domain.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Benefit.DataTransfer.ViewModels.Admin
{
    public class ModerationProduct
    {
        public ModerationProduct()
        {
        }

        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MaxLength(128)]
        public string UrlName { get; set; }
        [MaxLength(128)]
        [Required]
        public string Vendor { get; set; }
        [MaxLength(64)]
        [Required]
        public string OriginCountry { get; set; }
        [Required(AllowEmptyStrings = true, ErrorMessage = "Опис обовязковий для заповнення")]
        public string Description { get; set; }
        public double Price { get; set; }
        public int? AvailableAmount { get; set; }
        [Required]
        public string ShortDescription { get; set; }
        [Required]
        [MaxLength(128)]
        public string CategoryId { get; set; }
        [Required]
        [MaxLength(128)]
        public string CurrencyId { get; set; }
        public ProductParameterProduct[] ProductParameterProducts { get; set; }
    }
}
