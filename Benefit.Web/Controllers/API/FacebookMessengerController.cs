using System.Net;
using System.Web.Http;
using Benefit.Services;

namespace Benefit.Web.Controllers.API
{
    public class FacebookMessengerController : ApiController
    {
        public IHttpActionResult Get([FromUri(Name = "hub.mode")] string mode, [FromUri(Name = "hub.challenge")] string challenge, [FromUri(Name = "hub.verify_token")] string verifyToken)
        {
            if (mode == "subscribe" && verifyToken == SettingsService.Notifications.FacebookVerificationToken)
            {
                return Ok(challenge);
            }
            return StatusCode(HttpStatusCode.Forbidden);
        }
    }
}
