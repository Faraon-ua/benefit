using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public enum CategoryNavigationType
    {
        SellersOnly,
        SellersAndProducts
    }
    public class Category
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(64)]
        public string Name { get; set; }
        [Required]
        [MaxLength(128)]
        public string UrlName { get; set; }
        [Required]
        [MaxLength(32)]
        public string NavigationType { get; set; }
        [Required]
        [MaxLength(256)]
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int Order { get; set; }
        public bool IsActive{ get; set; }
        public bool IsVerified { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public string ParentCategoryId { get; set; }
        public virtual Category ParentCategory { get; set; }
        public ICollection<Category> ChildCategories { get; set; }
        [NotMapped]
        public List<Localization> Localizations { get; set; } 
    }
}
