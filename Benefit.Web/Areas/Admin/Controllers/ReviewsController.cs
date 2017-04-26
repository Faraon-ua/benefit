using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Web.Areas.Admin.Controllers.Base;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.AdminRoleName)]
    public class ReviewsController : AdminController
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            var reviews = db.Reviews
                .Include(entry => entry.ParentReview)
                .Include(entry => entry.Product.Category)
                .Include(entry => entry.Product.Seller)
                .Include(entry => entry.Seller).Where(entry => !entry.IsActive).OrderByDescending(entry=>entry.Stamp);
            return View(reviews);
        }

        [HttpPost]
        public ActionResult AcceptReview(string reviewId)
        {
            var review = db.Reviews.Find(reviewId);
            review.IsActive = true;
            db.Entry(review).State = EntityState.Modified;
            db.SaveChanges();

            if (review.ProductId != null)
            {
                var product =
                    db.Products.Include(entry => entry.Reviews).FirstOrDefault(entry => entry.Id == review.ProductId);
                product.AvarageRating = (int) Math.Round(product.ApprovedReviews.Average(entry => entry.Rating.Value), 0);
                db.Entry(product).State = EntityState.Modified;
            }
            if (review.SellerId != null)
            {
                var seller =
                    db.Sellers.Include(entry => entry.Reviews).FirstOrDefault(entry => entry.Id == review.SellerId);
                seller.AvarageRating = (int)Math.Round(seller.ApprovedReviews.Average(entry => entry.Rating.Value), 0);
                db.Entry(seller).State = EntityState.Modified;
            }
            db.SaveChanges();

            return Content(string.Empty);
        }

        [HttpPost]
        public ActionResult RemoveReview(string reviewId)
        {
            var review = db.Reviews.Find(reviewId);
            db.Reviews.Remove(review);
            db.SaveChanges();
            return Content(string.Empty);
        }
    }
}