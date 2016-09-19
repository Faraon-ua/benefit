﻿using System.ComponentModel.DataAnnotations;
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

        [NotMapped]
        public string ExpandedName
        {
            get
            {
                var current = this;
                var sb = new StringBuilder(RegionType + " " + Name_ua);
                while (current.Parent != null)
                {
                    sb.Append(" " + current.Parent.Name_ua + " " + current.Parent.RegionType);
                    current = current.Parent;
                }
                return sb.ToString().Trim();
            }
        }
    }
}
