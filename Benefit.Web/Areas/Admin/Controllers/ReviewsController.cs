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
            var reviews = db.Reviews.Where(entry => !entry.IsActive);
            return View(reviews);
        }

        [HttpPost]
        public ActionResult AcceptReview(string reviewId)
        {
            var review = db.Reviews.Find(reviewId);
            review.IsActive = true;
            db.Entry(review).State = EntityState.Modified;
            db.SaveChanges();

            var product =
                db.Products.Include(entry => entry.Reviews).FirstOrDefault(entry => entry.Id == review.ProductId);
            product.AvarageRating = (int)Math.Round(product.Reviews.Average(entry => entry.Rating), 0);
            db.Entry(product).State = EntityState.Modified;
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