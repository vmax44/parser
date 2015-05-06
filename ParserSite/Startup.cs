using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ParserSite.Startup))]
namespace ParserSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
