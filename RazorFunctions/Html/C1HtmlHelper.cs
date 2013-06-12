using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

using Composite.Core.Types;
using Composite.Core.Xml;
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

        public IHtmlString PageUrl(string id, object querystring = null)
        {
            var dict = Functions.ObjectToDictionary(querystring);

            return PageUrl(id, dict);
        }

        public IHtmlString PageUrl(string id, IDictionary<string, object> querystring)
        {
            string relativeUrl = "~/page(" + id + ")";
            string absoulteUrl = VirtualPathUtility.ToAbsolute(relativeUrl);

            if (querystring != null && querystring.Keys.Count > 0)
            {
                absoulteUrl += "?" + String.Join("&amp;", querystring.Select(kvp => kvp.Key + "=" + kvp.Value));
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

        public IHtmlString MediaUrl(Guid id, object querystring = null)
        {
            return MediaUrl(id.ToString(), querystring);
        }

        public IHtmlString MediaUrl(Guid id, IDictionary<string, string> querystring)
        {
            return MediaUrl(id.ToString(), querystring);
        }

        public IHtmlString MediaUrl(string keyPath, object querystring = null)
        {
            var dict = Functions.ObjectToDictionary(querystring);

            return MediaUrl(keyPath, dict);
        }

        public IHtmlString MediaUrl(string keyPath, IDictionary<string, object> querystring)
        {
            string relativeUrl = "~/media(" + keyPath + ")";
            string absoulteUrl = VirtualPathUtility.ToAbsolute(relativeUrl);

            if (querystring != null && querystring.Keys.Count > 0)
            {
                absoulteUrl += "?" + String.Join("&amp;", querystring.Select(kvp => kvp.Key + "=" + HttpUtility.UrlEncode(kvp.Value.ToString())));
            }

            return _helper.Raw(absoulteUrl);
        }

        public IHtmlString Document(XhtmlDocument xhtmlDocument)
        {
            return _helper.Raw(xhtmlDocument.ToString());
        }

        public IHtmlString Body(string xhtmlDocument)
        {
            var doc = XhtmlDocument.Parse(xhtmlDocument);

            return Body(doc);
        }

        public IHtmlString Body(XhtmlDocument xhtmlDocument)
        {
            var body = xhtmlDocument.Descendants().SingleOrDefault(el => el.Name.LocalName == "body");
            if (body != null)
            {
                using (var reader = body.CreateReader())
                {
                    reader.MoveToContent();

                    return _helper.Raw(reader.ReadInnerXml());
                }
            }

            return Document(xhtmlDocument);
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
