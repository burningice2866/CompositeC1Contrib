using CompositeC1Contrib.Email.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(OwinStartupHandler))]

namespace CompositeC1Contrib.Email.SignalR
{
    public class OwinStartupHandler
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}
