using System.Net;
using System.Web.Http;
using Benefit.Services;
using Benefit.Services.ExternalApi;
using System;

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
                        FacebookService.ReceiveMessage(message, sender);
                    }
                }
            }
            return Ok();
        }
    }
}
