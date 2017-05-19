using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Pawprint.Startup))]
namespace Pawprint
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
