using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class InfoPage
    {
        public string Id { get; set; }
        [Required]
        [MaxLength(250)]
        public string Name { get; set; }
        [MaxLength(70)]
        public string Title { get; set; }
        [MaxLength(250)]
        [Required]
        public string UrlName { get; set; }
        [MaxLength(512)]
        public string ShortContent { get; set; }
        [MaxLength(160)]
        public string Keywords { get; set; }
        public string Content { get; set; }
        [MaxLength(128)]
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsNews { get; set; }
        public int Order { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        [NotMapped]
        public List<Localization> Localizations { get; set; }
    }
}
