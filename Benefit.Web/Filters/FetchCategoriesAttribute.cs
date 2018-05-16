﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using WebGrease.Css.Extensions;

namespace Benefit.Web.Filters
{
    public class FetchCategoriesAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            List<Category> categories = null;
            var seller = filterContext.Controller.ViewBag.Seller as Seller;
            if (seller != null)
            {
                if (HttpRuntime.Cache["Categories" + seller.Id] != null)
                {
                    categories = filterContext.Controller.ViewBag.Categories = HttpRuntime.Cache["Categories" + seller.Id];
                }
                else
                {
                    using (var db = new ApplicationDbContext())
                    {
                        categories = db.SellerCategories.Include(entry => entry.Category.Products)
                           .Where(entry => entry.SellerId == seller.Id).Select(entry => entry.Category).Where(entry => entry.Products.Any()).ToList();
                        HttpRuntime.Cache.Insert("Categories" + seller.Id, categories, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(6));
                    }
                }
            }
            else
            {
                if (HttpRuntime.Cache["Categories"] != null)
                {
                    categories = filterContext.Controller.ViewBag.Categories = HttpRuntime.Cache["Categories"];
                }
                else
                {
                    using (var db = new ApplicationDbContext())
                    {
                        categories = db.Categories
                           .Include(entry => entry.Products)
                           .Include(entry => entry.ChildCategories.Select(cat => cat.Products))
                           .Include(entry => entry.ChildCategories.Select(cat => cat.ChildCategories.Select(ch => ch.Products)))
                           .Where(
                               entry =>
                                   entry.ParentCategoryId == null && entry.IsActive && !entry.IsSellerCategory)
                           .OrderBy(entry => entry.Order)
                           .ToList();
                        categories.ForEach(entry =>
                        {
                            entry.ChildCategories.ForEach(child =>
                            {
                                child.ChildCategories = child.ChildCategories.Where(cat => cat.IsActive && cat.Products.Any()).OrderBy(cat => cat.Order).ToList();
                            });
                            entry.ChildCategories = entry.ChildCategories.Where(cat => cat.IsActive && (cat.ChildCategories.Any() || cat.Products.Any())).OrderBy(cat => cat.Order).ToList();
                        });
                        categories = categories.Where(entry => entry.ChildCategories.Any() || entry.Products.Any()).ToList();
                        HttpRuntime.Cache.Insert("Categories", categories, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(6));
                    }
                }
            }
            filterContext.Controller.ViewBag.Categories = categories;
        }
    }
}