using Benefit.Services;
using System.Net;
using System.Web.Http;

namespace Benefit.RestApi.Controllers
{
    public class NotificationsController : ApiController
    {
        [HttpGet]
        public IHttpActionResult webhook([FromUri(Name = "hub.mode")] string mode,
            [FromUri(Name = "hub.challenge")] string challenge, [FromUri(Name = "hub.verify_token")] string verifyToken)
        {
            if (mode == "subscribe" && verifyToken == SettingsService.Notifications.FacebookVerificationToken)
            {
                return Ok(challenge);
            }
            return StatusCode(HttpStatusCode.Forbidden);
        }
    }
}
