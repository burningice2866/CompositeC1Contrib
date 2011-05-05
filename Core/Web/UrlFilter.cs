using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using Composite.Data;
using Composite.Data.Types;

using C1UrlUtils = Composite.Core.WebClient.UrlUtils;

namespace CompositeC1Contrib.Web
{
    public class UrlFilter : BaseResponseFilter
    {
        private static readonly Regex _pageUriRegEx;
        private static readonly Regex _hrefRegEx = new Regex("(?<=href\\s*=\\s*(?<quote>['\"])).*?(?=\\k<quote>\\s*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static UrlFilter()
        {
            var prepends = DataLocalizationFacade.UrlMappingNames;

            using (var data = new DataConnection())
            {
                var websites = data.Get<IPageStructure>()
                    .Where(p => p.ParentId == Guid.Empty)
                    .Select(ws => data.Get<IPage>().SingleOrDefault(o => o.Id == ws.Id))
                    .Where(p => p != null)
                    .Select(p => p.UrlTitle);                    

                prepends = prepends.Concat(websites).ToList();
            }

            var _pageUri = String.Format("(?<![\\w/])/(({0})/.+\\.aspx|({0}).aspx)", String.Join("|", prepends));
            if (!String.IsNullOrEmpty(C1UrlUtils.PublicRootPath))
            {
                _pageUri = _pageUri.Insert(10, C1UrlUtils.PublicRootPath);
            }

            _pageUriRegEx = new Regex(_pageUri, RegexOptions.Compiled | RegexOptions.IgnoreCase);
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
            var hrefs = _hrefRegEx.Matches(unparsedString);
            if (hrefs.Count == 0)
            {
                return unparsedString;
            }

            var builder = new StringBuilder();
            int startIndex = 0;
            var provider = (BaseSiteMapProvider)SiteMap.Provider;
            string host = String.Format("{0}://{1}", Context.Request.Url.Scheme, Context.Request.Url.Host);

            foreach (Match href in hrefs)
            {
                var c1PageLink = _pageUriRegEx.Match(href.Value);
                if (c1PageLink.Success)
                {
                    string url = href.Value.ToLower();
                    if (String.IsNullOrEmpty(url))
                    {
                        continue;
                    }

                    builder.Append(unparsedString.Substring(startIndex, href.Index - startIndex));

                    var pageUrl = PageUrl.Parse(String.Concat(host, url));
                    if (pageUrl != null)
                    {
                        var guid = pageUrl.PageId;
                        var ci = pageUrl.Locale;
                        var node = provider.FindSiteMapNodeFromKey(guid.ToString(), ci);

                        if (node != null)
                        {
                            url = Regex.Replace(url, c1PageLink.Value, node.Url, RegexOptions.IgnoreCase);
                        }
                        else
                        {
                            url = href.Value;
                        }
                    }
                    else
                    {
                        url = href.Value;
                    }

                    builder.Append(url);
                    startIndex = href.Index + href.Length;
                }
            }

            builder.Append(unparsedString.Substring(startIndex, unparsedString.Length - startIndex));

            return builder.ToString();
        }
    }
}
