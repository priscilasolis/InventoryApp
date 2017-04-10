using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(InventoryApp.Startup))]
namespace InventoryApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
