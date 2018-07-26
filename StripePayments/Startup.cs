using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(StripePayments.Startup))]
namespace StripePayments
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
