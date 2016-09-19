﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum ProductParameterType
    {
        
    }
    public class ProductParameter
    {
        public ProductParameter()
        {
            ChildProductParameters = new Collection<ProductParameter>();
            ProductParameterValues = new Collection<ProductParameterValue>();
        }
        [Key]
        public string Id { get; set; }
        [MaxLength(64)]
        [Required]
        public string Name { get; set; }
        [MaxLength(64)]
        [Required]
        public string UrlName { get; set; }
        public int? Order { get; set; }
        public bool DisplayInFilters { get; set; }
        [MaxLength(16)]
        public string MeasureUnit { get; set; }
        [MaxLength(32)]
        public string Type { get; set; }
        public bool IsVerified { get; set; }
        [MaxLength(128)]
        public string AddedById { get; set; }
        public virtual ApplicationUser AddedBy { get; set; }
        [MaxLength(128)]
        public string ParentProductParameterId { get; set; }
        public virtual ProductParameter ParentProductParameter { get; set; }
        [MaxLength(128)]
        public string CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<ProductParameter> ChildProductParameters { get; set; } 
        public virtual ICollection<ProductParameterValue> ProductParameterValues { get; set; }
        public virtual ICollection<ProductParameterProduct> ProductParameterProducts { get; set; }


        /*[MaxLength(32)]
        public string Type { get; set; }
        [MaxLength(128)]
        public string Value { get; set; }
        [MaxLength(10)]
        public bool IsVerified { get; set; }
        [MaxLength(128)]
        public string ProductId { get; set; }
        public virtual Product Product { get; set; }*/
    }
}
