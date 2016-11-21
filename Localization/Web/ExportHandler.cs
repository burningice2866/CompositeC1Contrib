using System;
using System.Web;

using CompositeC1Contrib.Localization.ImportExport;

namespace CompositeC1Contrib.Localization.Web
{
    public class ExportHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var languages = context.Request.QueryString["languages"].Split(',');
            var resourceSets = context.Request.QueryString["resourceSets"].Split(',');
            var ns = (context.Request.QueryString["ns"] ?? String.Empty);

            var exporter = new ResourceExporter(languages, resourceSets, ns);
            var xml = exporter.Export().ToXml();

            context.Response.Clear();

            context.Response.AddHeader("Content-Disposition", "attachment; filename=export.xml");
            context.Response.AddHeader("Content-Type", "application/xml");

            context.Response.Write(xml);
        }
    }
}
