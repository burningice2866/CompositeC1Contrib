using System;
using System.Collections.Generic;
using System.IO;
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

        public void Dispose() { }

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
                if (page.AppRelativeVirtualPath == "~/Renderers/Page.aspx")
                {
                    string masterPageFile = resolveMasterPagePath(iPage, page.Request);

                    if (masterPageFile != null)
                    {
                        page.AppRelativeVirtualPath = "~/";
                        page.MasterPageFile = masterPageFile;
                    }
                }
            }
        }

        private static string resolveInDirectory(string directory, string template, HttpRequest request)
        {
            var pathProvider = HostingEnvironment.VirtualPathProvider;
            var specialMode = getSpecialMode(request);

            if (specialMode != null)
            {
                var specialModeMaster = Path.Combine(directory, String.Format("{0}_{1}.master", template, specialMode));
                if (pathProvider.FileExists(specialModeMaster))
                {
                    return specialModeMaster;
                }
            }

            var masterFile = Path.Combine(directory, template + ".master");
            if (pathProvider.FileExists(masterFile))
            {
                return masterFile;
            }

            return null;
        }

        private static string getSpecialMode(HttpRequest request)
        {
            var specielPageModes = new[] { "print", "rss", "atom", "mobile" };
            var qs = request.QueryString;

            if (qs.AllKeys.Length > 0 && qs.Keys[0] == null)
            {
                if (specielPageModes.Contains(qs[0]))
                {
                    return qs[0];
                }
            }

            if (request.Browser.IsMobileDevice)
            {
                return "mobile";
            }

            return null;
        }

        private static string resolveMasterPagePath(IPage page, HttpRequest request)
        {
            var rootTemplateDir = "~/App_Data/PageTemplates/";

            var dirsToTry = new List<string>
            {
                rootTemplateDir
            };

            using (var data = new DataConnection())
            {
                var template = data.Get<IXmlPageTemplate>().Single(t => t.Id == page.TemplateId);
                var path = template.PageTemplateFilePath;
                var templateName = path.Replace(".xml", String.Empty).Remove(0, 1);

                var siteId = data.SitemapNavigator.GetPageNodeById(page.Id).GetPageIds(SitemapScope.Level1).First().ToString();
                dirsToTry.Insert(0, Path.Combine(rootTemplateDir, siteId));

                foreach (var dir in dirsToTry)
                {
                    var masterFile = resolveInDirectory(dir, templateName, request);
                    if (masterFile != null)
                    {
                        return masterFile;
                    }
                }
            }

            return null;
        }
    }
}
