using System;
using System.Collections.Generic;
using System.Web;
using System.Web.WebPages.Html;

using Composite.Core.Types;
using Composite.Data.Types;

namespace CompositeC1Contrib.RazorFunctions
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
            return PageUrl(page.Id);
        }

        public IHtmlString PageUrl(Guid id)
        {
            return new HtmlString("~/page(" + id + ")");
        }

        public IHtmlString MediaUrl(IMediaFile mediaFile)
        {
            return MediaUrl(mediaFile.KeyPath);
        }

        public IHtmlString MediaUrl(Guid id)
        {
            return MediaUrl(id.ToString());
        }

        public IHtmlString MediaUrl(string keyPath)
        {
            return new HtmlString("~/media(" + keyPath + ")");
        }

        public IHtmlString Function(string name)
        {
            return Function(name, null);
        }

        public IHtmlString Function(string name, IDictionary<string, object> @params)
        {
            object result = Functions.ExecuteFunction(name, @params);

            return convertFunctionResult(result);
        }

        public IHtmlString Function(string name, object @params)
        {
            object result = Functions.ExecuteFunction(name, @params);

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
