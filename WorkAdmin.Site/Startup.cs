using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WorkAdmin.Site.Startup))]
namespace WorkAdmin.Site
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
