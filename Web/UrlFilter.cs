using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web
{
    public class UrlFilter : BaseResponseFilter
    {
        private static readonly Regex _pageUriRegEx;

        static UrlFilter()
        {
            var prepends = DataLocalizationFacade.UrlMappingNames.ToList();

            using (var data = new DataConnection())
            {
                var websites = data.Get<IPageStructure>().Where(p => p.ParentId == Guid.Empty);
                foreach (var site in websites)
                {
                    var page = data.Get<IPage>().Single(p => p.Id == site.Id);
                    prepends.Add(page.UrlTitle);
                }
            }

            var _pageUri = String.Format(@"(?<![\w/])/(({0})/.+\.aspx|({0}).aspx)", String.Join("|", prepends));

            _pageUriRegEx = new Regex(_pageUri, RegexOptions.Compiled);
        }

        public UrlFilter(Stream responseStream, HttpContext ctx) : base(responseStream, ctx) { }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var encoding = Encoding.UTF8;

            var s = encoding.GetString(buffer, offset, count);
            s = FixUrls(s);

            buffer = encoding.GetBytes(s);

            base.Write(buffer, 0, buffer.Length);
        }

        public string FixUrls(string unparsedString)
        {
            var matches = _pageUriRegEx.Matches(unparsedString);
            if (matches.Count == 0)
            {
                return unparsedString;
            }

            var builder = new StringBuilder();
            int startIndex = 0;

            foreach (Match match in matches)
            {
                string url = match.Value;
                if (String.IsNullOrEmpty(url))
                {
                    continue;
                }

                builder.Append(unparsedString.Substring(startIndex, match.Index - startIndex));

                url = url.ToLower();

                string fullUrl = String.Format("{0}://{1}{2}", Context.Request.Url.Scheme, Context.Request.Url.Host, url);
                var pageUrl = PageUrl.Parse(fullUrl);

                if (pageUrl != null)
                {
                    var provider = (BaseSiteMapProvider)SiteMap.Provider;
                    var guid = pageUrl.PageId;
                    var ci = pageUrl.Locale;

                    url = provider.FindSiteMapNodeFromKey(guid.ToString(), ci).Url;

                    builder.Append(url);
                }
                else
                {
                    builder.Append(match.Value);
                }

                startIndex = match.Index + match.Length;
            }

            builder.Append(unparsedString.Substring(startIndex, unparsedString.Length - startIndex));

            return builder.ToString();
        }
    }
}
