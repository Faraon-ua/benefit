using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Services.Domain;
using WebGrease.Css.Extensions;

namespace Benefit.Web.Filters
{
    public class FetchCategoriesAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            List<Category> categories = null;
            List<CategoryVM> categoriesVM = null;
            var seller = filterContext.Controller.ViewBag.Seller as Seller;
            if (seller != null)
            {
                if (HttpRuntime.Cache["Categories" + seller.Id] != null)
                {
                    categoriesVM = filterContext.Controller.ViewBag.Categories = HttpRuntime.Cache["Categories" + seller.Id];
                }
                else
                {
                    var sellerService = new SellerService();
                    var sellerCats = sellerService.GetAllSellerCategories(seller.UrlName);
                    categories = sellerCats.Where(entry => entry.ParentCategoryId == null).ToList();
                    if (categories.Count == 1)
                    {
                        categories = categories.SelectMany(entry => entry.ChildCategories)
                            .Where(entry => entry.IsActive && !entry.IsSellerCategory).ToList();
                    }

                    categoriesVM = categories.MapToVM().OrderByDescending(entry => entry.ChildCategories.Any()).ToList();
                    HttpRuntime.Cache.Insert("Categories" + seller.Id, categoriesVM, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(6));

                    //using (var db = new ApplicationDbContext())
                    //{
                    //    categories = db.SellerCategories.Include(entry => entry.Category.Products)
                    //       .Where(entry => entry.SellerId == seller.Id).Select(entry => entry.Category).Where(entry => entry.Products.Any()).ToList();
                    //}
                }
            }
            else
            {
                if (HttpRuntime.Cache["Categories"] != null)
                {
                    categoriesVM = filterContext.Controller.ViewBag.Categories = HttpRuntime.Cache["Categories"];
                }
                else
                {
                    using (var db = new ApplicationDbContext())
                    {
                        categories = db.Categories
                            .Include(entry => entry.ChildCategories.Select(cat =>cat.ChildCategories.Select(ch=>ch.MappedCategories)))
                            .Where(
                                entry =>
                                    entry.ParentCategoryId == null && entry.IsActive && !entry.IsSellerCategory)
                            .OrderBy(entry => entry.Order)
                            .ToList();
                        categories.ForEach(entry =>
                        {
                            entry.ChildCategories.ForEach(child =>
                            {
                                child.ChildCategories = child.ChildCategories.Where(cat => cat.IsActive).OrderBy(cat => cat.Order).ToList();
                            });
                            entry.ChildCategories = entry.ChildCategories.Where(cat => cat.IsActive).OrderBy(cat => cat.Order).ToList();
                        });
                        categories = categories.ToList();
                        categoriesVM = categories.MapToVM();
                        HttpRuntime.Cache.Insert("Categories", categoriesVM, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(6));
                    }
                }
            }
            filterContext.Controller.ViewBag.Categories = categoriesVM;
        }
    }
}