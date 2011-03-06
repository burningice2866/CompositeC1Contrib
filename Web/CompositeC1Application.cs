using System;
using System.Web;
using System.Web.Routing;

using Composite.Core.WebClient;

using CompositeC1Contrib.Web.Mvc;

namespace CompositeC1Contrib.Web
{
    public class CompositeC1Application : HttpApplication
    {
        public override void Init()
        {
            BeginRequest += new EventHandler(CompositeC1Application_BeginRequest);
            EndRequest += new EventHandler(CompositeC1Application_EndRequest);
            Error += new EventHandler(CompositeC1Application_Error);

            base.Init();
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            ApplicationLevelEventHandlers.LogRequestDetails = false;
            ApplicationLevelEventHandlers.LogApplicationLevelErrors = true;

            ApplicationLevelEventHandlers.Application_Start(sender, e);

            if (AppSettings.UseMvcForContentRendering)
            {
                var routes = RouteTable.Routes;
                routes.Add(new ContentRoute());
            }
        }


        protected void Application_End(object sender, EventArgs e)
        {
            ApplicationLevelEventHandlers.Application_End(sender, e);
        }

        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            return ApplicationLevelEventHandlers.GetVaryByCustomString(context, custom) ?? base.GetVaryByCustomString(context, custom);
        }

        private void CompositeC1Application_Error(object sender, EventArgs e)
        {
            ApplicationLevelEventHandlers.Application_Error(sender, e);
        }

        private void CompositeC1Application_EndRequest(object sender, EventArgs e)
        {
            ApplicationLevelEventHandlers.Application_EndRequest(sender, e);
        }

        private void CompositeC1Application_BeginRequest(object sender, EventArgs e)
        {
            ApplicationLevelEventHandlers.Application_BeginRequest(sender, e);
        }
    }
}
