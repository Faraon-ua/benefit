﻿using System.Collections.Generic;
using System.Linq;
using Benefit.Domain.DataAccess;
using System.Data.Entity;

namespace Benefit.Domain.Models.ModelExtensions
{
    public static class CategoryExt
    {
        public static IEnumerable<Category> GetAllChildrenRecursively(this Category category)
        {
            using (var db = new ApplicationDbContext())
            {
                foreach (var cat in db.Categories.AsNoTracking().Include(entry=>entry.ChildCategories).Include(entry=>entry.SellerCategories).Include(entry => entry.MappedCategories).Where(entry => entry.ParentCategoryId == category.Id))
                {
                    yield return cat;

                    if (cat.ChildCategories.Count > 0)
                    {
                        foreach (var childCat in cat.GetAllChildrenRecursively())
                        {
                            yield return childCat;
                        }
                    }
                }
            }
        }
    }
}
