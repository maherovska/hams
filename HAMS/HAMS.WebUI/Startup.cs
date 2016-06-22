using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HAMS.WebUI.Startup))]
namespace HAMS.WebUI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
