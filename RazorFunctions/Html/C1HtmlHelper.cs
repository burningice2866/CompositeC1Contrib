using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;
using System.Xml.Linq;

using Composite.Core.Types;
using Composite.Data.Types;

namespace CompositeC1Contrib.RazorFunctions.Html
{
    public class C1HtmlHelper
    {
        private HtmlHelper _helper;

        public C1HtmlHelper(HtmlHelper helper)
        {
            _helper = helper;
        }

        public IHtmlString PageUrl(IPage page)
        {
            return PageUrl(page.Id.ToString());
        }

        public IHtmlString PageUrl(IPage page, object querystring)
        {
            return PageUrl(page.Id.ToString(), querystring);
        }

        public IHtmlString PageUrl(IPage page, IDictionary<string, string> querystring)
        {
            return PageUrl(page.Id.ToString(), querystring);
        }

        public IHtmlString PageUrl(string id)
        {
            return PageUrl(id, null);
        }

        public IHtmlString PageUrl(string id, object querystring)
        {
            return PageUrl(id, Functions.ObjectToDictionary(querystring));
        }

        public IHtmlString PageUrl(string id, IDictionary<string, string> querystring)
        {
            string relativeUrl = "~/page(" + id + ")";
            string absoulteUrl = VirtualPathUtility.ToAbsolute(relativeUrl);

            if (querystring != null && querystring.Keys.Count > 0)
            {
                absoulteUrl += "?" + String.Join("&", querystring.Select(kvp => kvp.Key + "=" + kvp.Value));
            }

            return _helper.Raw(absoulteUrl);
        }

        public IHtmlString MediaUrl(IMediaFile mediaFile)
        {
            return MediaUrl(mediaFile.KeyPath);
        }

        public IHtmlString MediaUrl(IMediaFile mediaFile, object querystring)
        {
            return MediaUrl(mediaFile.KeyPath, querystring);
        }

        public IHtmlString MediaUrl(IMediaFile mediaFile, IDictionary<string, string> querystring)
        {
            return MediaUrl(mediaFile.KeyPath, querystring);
        }

        public IHtmlString MediaUrl(Guid id)
        {
            return MediaUrl(id.ToString(), null);
        }

        public IHtmlString MediaUrl(Guid id, object querystring)
        {
            return MediaUrl(id.ToString(), querystring);
        }

        public IHtmlString MediaUrl(Guid id, IDictionary<string, string> querystring)
        {
            return MediaUrl(id.ToString(), querystring);
        }

        public IHtmlString MediaUrl(string keyPath)
        {
            return MediaUrl(keyPath, null);
        }

        public IHtmlString MediaUrl(string keyPath, object querystring)
        {
            return MediaUrl(keyPath, Functions.ObjectToDictionary(querystring));
        }       

        public IHtmlString MediaUrl(string keyPath, IDictionary<string, string> querystring)
        {
            string relativeUrl = "~/media(" + keyPath + ")";
            string absoulteUrl = VirtualPathUtility.ToAbsolute(relativeUrl);

            if (querystring != null && querystring.Keys.Count > 0)
            {
                absoulteUrl += "?" + String.Join("&", querystring.Select(kvp => kvp.Key + "=" + kvp.Value));
            }

            return _helper.Raw(absoulteUrl);
        }

        public IHtmlString BodySection(string xhtmlDocument)
        {
            var doc = XElement.Parse(xhtmlDocument);

            var body = doc.Descendants().SingleOrDefault(el => el.Name.LocalName == "body");
            if (body != null)
            {
                using (var reader = body.CreateReader())
                {
                    reader.MoveToContent();

                    return new HtmlString(reader.ReadInnerXml());
                }
            }

            return new HtmlString(xhtmlDocument);
        }

        public IHtmlString Function(string name)
        {
            return Function(name, null);
        }

        public IHtmlString Function(string name, object parameters)
        {
            return Function(name, Functions.ObjectToDictionary(parameters));
        }

        public IHtmlString Function(string name, IDictionary<string, object> parameters)
        {
            object result = Functions.ExecuteFunction(name, parameters);

            return convertFunctionResult(result);
        }

        private static IHtmlString convertFunctionResult(object result)
        {
            string resultAsString = ValueTypeConverter.Convert<string>(result);
            if (resultAsString != null)
            {
                return new HtmlString(resultAsString);
            }

            throw new InvalidOperationException("Function doesn't return string value");
        }
    }
}
