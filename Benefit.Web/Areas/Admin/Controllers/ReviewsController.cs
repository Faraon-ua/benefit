using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Benefit.Common.Constants;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Web.Areas.Admin.Controllers.Base;

namespace Benefit.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = DomainConstants.AdminRoleName)]
    public class ReviewsController : AdminController
    {
        public ActionResult Index()
        {
            using (var db = new ApplicationDbContext())
            {
                var reviews = db.Reviews
                .Include(entry => entry.ParentReview)
                .Include(entry => entry.Product.Category)
                .Include(entry => entry.Product.Seller)
                .Include(entry => entry.Seller).Where(entry => !entry.IsActive).OrderByDescending(entry => entry.Stamp).ToList();
                return View(reviews);
            }
        }

        [HttpPost]
        public ActionResult AcceptReview(string reviewId)
        {
            using (var db = new ApplicationDbContext())
            {
                var review = db.Reviews.Find(reviewId);
                review.IsActive = true;
                db.Entry(review).State = EntityState.Modified;
                db.SaveChanges();

                if (review.ProductId != null)
                {
                    var product =
                        db.Products.Include(entry => entry.Reviews).FirstOrDefault(entry => entry.Id == review.ProductId);
                    product.AvarageRating = (int)Math.Round(product.ApprovedReviews.Average(entry => entry.Rating.Value), 0);
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
        }

        [HttpPost]
        public ActionResult RemoveReview(string reviewId)
        {
            using (var db = new ApplicationDbContext())
            {
                var review = db.Reviews.Find(reviewId);
                db.Reviews.Remove(review);
                db.SaveChanges();
                return Content(string.Empty);
            }
        }

        public ActionResult CreateOrUpdate(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var review = db.Reviews.Find(id);
                if (review == null) return HttpNotFound();
                return View(review);
            }
        }

        [HttpPost]
        public ActionResult CreateOrUpdate(Review review)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    db.Entry(review).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Відгук збережено";
                    return RedirectToAction("CreateOrUpdate", new { id = review.Id });
                }

                return View(review);
            }
        }
    }
}