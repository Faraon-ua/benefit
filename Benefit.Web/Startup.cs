using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Benefit.Web.Startup))]
namespace Benefit.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
