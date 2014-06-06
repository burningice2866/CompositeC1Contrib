using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;

using Composite.Data;

using CompositeC1Contrib.Favorites.Data.Types;

namespace CompositeC1Contrib.Favorites.Web
{
    public class FavoritesRewriteModule : IHttpModule
    {
        public void Dispose() { }

        public void Init(HttpApplication app)
        {
            app.BeginRequest += app_BeginRequest;
        }

        private static void app_BeginRequest(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender).Context;
            var request = ctx.Request;

            if (!request.Path.EndsWith("Composite/services/WysiwygEditor/XhtmlTransformations.asmx") || request.RequestType != "POST" || request.Headers["x-supress-rewrite"] == "true")
            {
                return;
            }

            string action = ctx.Request.Headers["SOAPAction"];
            if (action != "http://www.composite.net/ns/management/GetFunctionInfo")
            {
                return;
            }

            string xmlData;
            using (var reader = new StreamReader(ctx.Request.InputStream))
            {
                xmlData = reader.ReadToEnd();
            }

            var functionName = XElement.Parse(xmlData).Descendants().Single(el => el.Name.LocalName == "functionName").Value;
            if (functionName.StartsWith("__Favorites"))
            {
                var name = functionName.Substring(12, functionName.Length - 12);

                using (var data = new DataConnection())
                {
                    var favorite = data.Get<IFavoriteFunction>().SingleOrDefault(f => f.Name == name);
                    if (favorite != null)
                    {
                        xmlData = xmlData.Replace(functionName, favorite.FunctionName);
                    }
                }
            }

            var bytesToWrite = Encoding.UTF8.GetBytes(xmlData);

            var req = (HttpWebRequest)WebRequest.Create(ctx.Request.Url);
            req.Method = "POST";
            req.Accept = "text/xml";
            req.ContentType = "text/xml;charset=\"utf-8\"";
            req.Headers.Add("x-supress-rewrite", "true");
            req.ContentLength = bytesToWrite.Length;

            CopyHeader(ctx.Request.Headers, req.Headers);

            using (var requestStream = req.GetRequestStream())
            {
                requestStream.Write(bytesToWrite, 0, bytesToWrite.Length);

                using (var response = req.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        CopyHeader(response.Headers, ctx.Response.Headers);

                        stream.CopyTo(ctx.Response.OutputStream);

                        ctx.Response.ContentType = response.ContentType;
                        ctx.Response.End();
                    }
                }
            }
        }

        private static void CopyHeader(NameValueCollection from, NameValueCollection to)
        {
            foreach (var key in from.AllKeys)
            {
                var value = from[key];

                try
                {
                    to.Add(key, value);
                }
                catch { }
            }
        }
    }
}
