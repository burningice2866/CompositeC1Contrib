using System;
using System.Web;
using System.Web.UI;

using Composite.Core.PageTemplates;
using Composite.Core.WebClient;
using Composite.Core.WebClient.Renderings.Page;

using NewRelicAgent = NewRelic.Api.Agent.NewRelic;

namespace CompositeC1Contrib.NewRelic
{
    public class NewRelicHttpModule : IHttpModule
    {
        public void Dispose() { }

        public void Init(HttpApplication app)
        {
            app.PostMapRequestHandler += app_PostMapRequestHandler;
        }

        static void app_PostMapRequestHandler(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;

            var handler = ctx.CurrentHandler as Page;
            if (handler == null)
            {
                return;
            }

            handler.Load += (o, args) =>
            {
                var c1Page = PageRenderer.CurrentPage;
                if (c1Page != null)
                {
                    var template = PageTemplateFacade.GetPageTemplate(c1Page.TemplateId);
                    var title = template.Title;

                    NewRelicAgent.SetTransactionName("C1 Page", title);
                }
                else
                {
                    var adminPath = UrlUtils.AdminRootPath;
                    var requestPath = ctx.Request.Url.LocalPath;

                    if (!requestPath.StartsWith(adminPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    var newRelic = handler.Request.QueryString["newrelic"];
                    if (newRelic == null || !newRelic.Equals("true"))
                    {
                        NewRelicAgent.DisableBrowserMonitoring(true);
                    }
                }
            };
        }
    }
}
