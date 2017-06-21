using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benefit.DataTransfer.ApiDto.Request
{
    public class FacebookWebhookIngest
    {
        public string hub.mode { get; set; }
        public string hub.challenge { get; set; }
        public string hub.verify_token { get; set; }
    }
}
