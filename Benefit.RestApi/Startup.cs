﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Benefit.RestApi.Startup))]

namespace Benefit.RestApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }
    }
}
