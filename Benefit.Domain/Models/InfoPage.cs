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
        [MaxLength(64)]
        public string Name { get; set; }
        public string Content { get; set; }
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
