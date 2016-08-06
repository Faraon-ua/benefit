using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class Filter
    {
        public string Id { get; set; }
        [MaxLength(64)]
        public string Name { get; set; }
        [MaxLength(64)]
        public string UrlName { get; set; }
        [MaxLength(32)]
        public string Type { get; set; }
        [MaxLength(128)]
        public string Value { get; set; }
        public bool IsVerified { get; set; }
        public string CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public string ParentFilterId { get; set; }
        public virtual Filter ParentFilter { get; set; }
    }
}
