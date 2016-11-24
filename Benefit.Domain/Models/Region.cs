using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Benefit.Domain.Models
{
    public class Region
    {
        [Key]
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public virtual Region Parent { get; set; }
        [MaxLength(6)]
        public string PostalCode { get; set; }
        [MaxLength(120)]
        [Index]
        public string Name_ru { get; set; }
        [MaxLength(10)]
        public string RegionType { get; set; }
        public short RegionLevel { get; set; }
        [MaxLength(120)]
        [Index]
        public string Name_ua { get; set; }
        [MaxLength(120)]
        public string Url { get; set; }

        private string _expandedName;
        [NotMapped]
        public string ExpandedName
        {
            get
            {
                if (_expandedName != null)
                    return _expandedName;

                var containsBracket = false;

                var current = this;
                var sb = new StringBuilder(RegionType + " " + Name_ua);
                while (current.Parent != null && current.Parent.RegionLevel > 0)
                {
                    if (current.Id == Id)
                    {
                        sb.Append(" (");
                        containsBracket = true;
                    }
                    else
                    {
                        sb.Append(" ");
                    }
                    sb.Append(current.Parent.Name_ua + " " + current.Parent.RegionType);
                    current = current.Parent;
                }
                if (containsBracket)
                {
                    sb.Append(")");
                }
                _expandedName = sb.ToString().Trim();
                return _expandedName;
            }
        }
    }
}
