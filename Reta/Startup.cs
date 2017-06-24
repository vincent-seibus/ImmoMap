using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Reta.Startup))]
namespace Reta
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
