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
            var seller = filterContext.Controller.ViewBag.Seller as Seller;
            if (seller != null)
            {
                if (HttpRuntime.Cache["Categories" + seller.Id] != null)
                {
                    categories = filterContext.Controller.ViewBag.Categories = HttpRuntime.Cache["Categories" + seller.Id];
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
                    HttpRuntime.Cache.Insert("Categories" + seller.Id, categories, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(6));

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
                    categories = filterContext.Controller.ViewBag.Categories = HttpRuntime.Cache["Categories"];
                }
                else
                {
                    using (var db = new ApplicationDbContext())
                    {
                        categories = db.Categories
                           .Include(entry => entry.Products)
                           .Include(entry => entry.ChildCategories.Select(cat => cat.Products.Select(pr=>pr.Seller)))
                           .Include(entry => entry.ChildCategories.Select(cat => cat.ChildCategories.Select(ch => ch.Products.Select(cp=>cp.Seller))))
                           .Where(
                               entry =>
                                   entry.ParentCategoryId == null && entry.IsActive && !entry.IsSellerCategory)
                           .OrderBy(entry => entry.Order)
                           .ToList();
                        categories.ForEach(entry =>
                        {
                            entry.ChildCategories.ForEach(child =>
                            {
                                child.ChildCategories = child.ChildCategories.Where(cat => cat.IsActive && cat.Products.Any(cc=> cc.IsActive && cc.Seller.IsActive)).OrderBy(cat => cat.Order).ToList();
                            });
                            entry.ChildCategories = entry.ChildCategories.Where(cat => cat.IsActive && (cat.ChildCategories.Any(chcat=>chcat.IsActive) || cat.Products.Any(pr => pr.IsActive && pr.Seller.IsActive))).OrderBy(cat => cat.Order).ToList();
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