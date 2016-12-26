using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Microsoft.AspNet.Identity;

namespace Benefit.Web.Controllers
{
    public class HelperController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Chat()
        {
            return PartialView();
        }

        public ActionResult Comments()
        {
            var allComments = db.Messages.Include(entry => entry.User).OrderByDescending(entry => entry.TimeStamp).ToList();
            return PartialView(allComments);
        }

        public ActionResult DeleteComment(string messageId)
        {
            var message = db.Messages.Find(messageId);
            db.Messages.Remove(message);
            db.SaveChanges();
            return Content("success");
        }

        public ActionResult AddChatMessage(string message)
        {
            var userId = User.Identity.GetUserId();
            var chatMessage = new Message()
            {
                Id = Guid.NewGuid().ToString(),
                Text = message,
                TimeStamp = DateTime.UtcNow,
                UserId = userId
            };
            db.Messages.Add(chatMessage);
            db.SaveChanges();
            var allComments = db.Messages.Include(entry => entry.User).OrderByDescending(entry => entry.TimeStamp).ToList();
            return PartialView("Comments", allComments);
        }
    }
}