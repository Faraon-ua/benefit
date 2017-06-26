using System.Net;
using System.Web.Http;
using Benefit.Services;
using Benefit.Services.ExternalApi;
using System;
using System.Diagnostics;
using System.Linq;
using Benefit.Domain.DataAccess;

namespace Benefit.Web.Controllers.API
{
    public class FacebookMessengerController : ApiController
    {
        FacebookService FacebookService = new FacebookService();

        [HttpGet]
        public IHttpActionResult Get([FromUri(Name = "hub.mode")] string mode,
            [FromUri(Name = "hub.challenge")] int challenge, [FromUri(Name = "hub.verify_token")] string verifyToken)
        {
            if (mode == "subscribe" && verifyToken == SettingsService.Facebook.VerificationToken)
            {
                return Ok(challenge);
            }
            return StatusCode(HttpStatusCode.Forbidden);
        }

        [HttpPost]
        public IHttpActionResult Post(dynamic collection)
        {
            // Make sure this is a page subscription
            if (collection["object"] == "page")
            {
                // Iterate over each entry - there may be multiple if batched
                foreach (var entry in collection["entry"])
                {
                    foreach (var messaging in entry["messaging"])
                    {
                        var sender = messaging["sender"]["id"].Value;
                        string message = Convert.ToString(messaging["message"]["text"].Value);
                        //for testing purposes
                        try
                        {
                            new Guid(message);
                            FacebookService.ReceiveMessage(message, sender);
                        }
                        catch
                        {
                            using (var db = new ApplicationDbContext())
                            {
                                var user = db.Users.FirstOrDefault(us => us.Email == message);
                                if (user != null)
                                {
                                    var msg = user.IsCardVerified
                                        ? "Your card is verified"
                                        : "Your card is NOT verified";
                                    FacebookService.SendMessage(sender, msg);
                                }
                                else
                                {
                                    FacebookService.SendMessage(sender, "User was not found");
                                }
                            }
                        }
                    }
                }
            }
            return Ok();
        }
    }
}
