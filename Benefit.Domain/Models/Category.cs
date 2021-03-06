﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Benefit.Domain.Models
{
    public enum CategoryNavigationType
    {
        SellersOnly,
        SellersAndProducts
    }
    public static class CategoryListExtentions
    {
        public static CategoryVM MapToVM(this Category category)
        {
            return AutoMapper.Mapper.Map<CategoryVM>(category);
        }
        public static List<CategoryVM> MapToVM(this IEnumerable<Category> categories)
        {
            var categoriesVM = categories.Select(entry => AutoMapper.Mapper.Map<CategoryVM>(entry)).ToList();
            return categoriesVM;
        }
        public static CategoryVM FindByUrlIdRecursively(this IEnumerable<CategoryVM> list, string url, string id)
        {
            var category = list.FirstOrDefault(entry => entry.UrlName == url || entry.Id == id || (entry.MappedCategories != null && entry.MappedCategories.Select(mc=>mc.UrlName).Contains(url)));
            if (category == null && !list.Any())
                return null;
            return category ?? FindByUrlIdRecursively(list.SelectMany(entry => entry.ChildCategories), url, id);
        }

        public static Category FindCatByUrlIdRecursively(this IEnumerable<Category> list, string url, string id)
        {
            var category = list.FirstOrDefault(entry => entry.UrlName == url || entry.Id == id || (entry.MappedCategories != null && entry.MappedCategories.Select(mc => mc.UrlName).Contains(url)));
            if (category == null && !list.Any())
                return null;
            return category ?? FindCatByUrlIdRecursively(list.SelectMany(entry => entry.ChildCategories), url, id);
        }

        public static IEnumerable<Category> SortByHierarchy(this List<Category> list, string parentId = null)
        {
            foreach (var cat in list.Where(entry=>entry.ParentCategoryId == parentId))
            {
                yield return cat;
                foreach (var childCat in list.SortByHierarchy(cat.Id))
                {
                    yield return childCat;
                }
            }
        }
    }
    [Serializable]
    public class CategoryVM
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Tag { get; set; }
        public string MetaDescription { get; set; }
        public string Description { get; set; }
        public string UrlName { get; set; }
        public bool ShowCartOnOrder { get; set; }
        public string ImageUrl { get; set; }
        public string BannerImageUrl { get; set; }
        public string BannerUrl { get; set; }
        public bool ChildAsFilters { get; set; }
        public int Order { get; set; }
        public CategoryVM ParentCategory { get; set; }
        public ICollection<CategoryVM> ChildCategories { get; set; }
        public ICollection<CategoryVM> MappedCategories { get; set; }
    }

    public class Category
    {
        public Category()
        {
            Products = new Collection<Product>();
            ProductOptions = new Collection<ProductOption>();
            ProductParameters = new Collection<ProductParameter>();
            ChildCategories = new Collection<Category>();
            SellerCategories = new Collection<SellerCategory>();
            MappedCategories = new Collection<Category>();
            ExportCategories = new Collection<ExportCategory>();
        }

        [Key]
        public string Id { get; set; }
        public string ExternalIds { get; set; }
        [Required]
        [MaxLength(64)]
        [Index]
        public string Name { get; set; }
        [MaxLength(200)]
        public string Title { get; set; }
        [Required]
        [MaxLength(128)]
        [Index(IsUnique = true)]
        public string UrlName { get; set; }
        public bool ChildAsFilters { get; set; }
        [MaxLength(256)]
        public string MetaDescription { get; set; }
        public string Description { get; set; }
        [MaxLength(250)]
        public string Tag { get; set; }
        public string ImageUrl { get; set; }
        [MaxLength(250)]
        public string BannerImageUrl { get; set; }
        [MaxLength(250)]
        public string BannerUrl { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public bool ShowCartOnOrder { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public bool IsSellerCategory { get; set; }
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        public string MappedParentCategoryId { get; set; }
        public Category MappedParentCategory { get; set; }
        public ICollection<Category> MappedCategories { get; set; }
        [MaxLength(128)]
        public string ParentCategoryId { get; set; }
        public virtual Category ParentCategory { get; set; }
        public ICollection<Category> ChildCategories { get; set; }
        public ICollection<SellerCategory> SellerCategories { get; set; }
        public ICollection<Product> Products { get; set; } 
        public ICollection<ProductParameter> ProductParameters { get; set; }
        public ICollection<ProductOption> ProductOptions { get; set; }
        public IList<ExportCategory> ExportCategories { get; set; }
        [NotMapped]
        public bool HasChildCategories { get; set; }

        [NotMapped]
        public List<Localization> Localizations { get; set; }

        [NotMapped]
        public string ExpandedName
        {
            get
            {
                var sb = new StringBuilder(Name);
                var parent = ParentCategory;
                while (parent != null)
                {
                    sb.Insert(0, parent.Name + " >> ");
                    parent = parent.ParentCategory;
                }
                return sb.ToString();
            }
        }

        [NotMapped]
        public string ExpandedSlashName { get; set; }

        [NotMapped]
        public int HierarchicalLevel
        {
            get
            {
                var level = 1;
                var parent = ParentCategory;
                while (parent != null)
                {
                    level++;
                    parent = parent.ParentCategory;
                }

                return level;
            }
        }
    }

    public class CategoryProductsCount
    {
        public Category Category { get; set; }
        public int ProductsCount { get; set; }
    }

    public class ParentCategoryChildren
    {
        public Category Category { get; set; }
        public List<Category> Children { get; set; }
    }

    public class CategoryComparer:IEqualityComparer<Category>
    {
        public bool Equals(Category x, Category y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Category obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
