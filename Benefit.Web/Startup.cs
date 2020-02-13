using Microsoft.Owin;
using Owin;
using System.Net;

[assembly: OwinStartupAttribute(typeof(Benefit.Web.Startup))]
namespace Benefit.Web
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
