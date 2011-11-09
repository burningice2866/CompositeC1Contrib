using System;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;

using Composite.Core.WebClient.Renderings.Page;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web
{
    public class MasterPageModule : IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.PostMapRequestHandler += new EventHandler(app_PostMapRequestHandler);
        }

        public void Dispose()
        {

        }

        private void app_PostMapRequestHandler(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var ctx = app.Context;

            var handler = ctx.CurrentHandler as Page;
            if (handler != null)
            {
                handler.PreInit += new EventHandler(handler_PreInit);
            }
        }

        private void handler_PreInit(object sender, EventArgs e)
        {
            var iPage = PageRenderer.CurrentPage;
            if (iPage != null)
            {
                var page = (Page)sender;

                var masterFileDirectory = "~/App_Data/PageTemplates/";
                IPageTemplate template = null;
                using (var data = new DataConnection())
                {
                    template = data.Get<IPageTemplate>().Single(t => t.Id == iPage.TemplateId);

                    var rootId = data.SitemapNavigator.GetPageNodeById(iPage.Id).GetPageIds(SitemapScope.Level1).First();
                    var dir = masterFileDirectory + rootId + "/";

                    if (HostingEnvironment.VirtualPathProvider.DirectoryExists(dir))
                    {
                        masterFileDirectory = dir;
                    }
                }

                var masterFile = masterFileDirectory + template.PageTemplateFilePath.Replace(".xml", String.Empty) + ".master";
                if (HostingEnvironment.VirtualPathProvider.FileExists(masterFile))
                {
                    var qs = page.Request.QueryString;
                    if (qs.AllKeys.Length > 0 && qs.Keys[0] == null && qs[0] == "print")
                    {
                        var printMasterFile = String.Format("{0}/{1}_print.master", masterFileDirectory, template.PageTemplateFilePath.Replace(".xml", String.Empty));

                        if (HostingEnvironment.VirtualPathProvider.FileExists(printMasterFile))
                        {
                            masterFile = printMasterFile;
                        }
                    }

                    page.AppRelativeVirtualPath = "~/";
                    page.MasterPageFile = masterFile;
                }
            }
        }
    }
}
