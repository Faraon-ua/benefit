using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benefit.DataTransfer.ApiDto.Request;
using Benefit.Services;

namespace Benefit.Web.Controllers
{
    public class NotificationsController : Controller
    {
        //
        // GET: /Notifications/
        public ActionResult webhook(FacebookWebhookIngest ingest)
        {
            if (ingest.mode == 'subscribe' && ingest.verify_token == = SettingsService.Notifications.FacebookVerificationToken)
            {
                return new HttpStatusCodeResult(200, ingest.challenge);
            }
        }
	}
}