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
    public class MediaUrlFilter : BaseResponseFilter
    {
        private static readonly Regex _regex = new Regex("/Renderers/ShowMedia\\.ashx\\?id=(?<id>[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-Z0-9]{12})(?<extra_query>.*?)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public MediaUrlFilter (Stream responseStream, HttpContext ctx) : base(responseStream, ctx) { }

        public override string Process(string s)
        {
            return MakeMediaUrlsFriendly(s);
        }

        public static string MakeMediaUrlsFriendly(string unparsedString)
        {
            StringBuilder builder = new StringBuilder();
            int startIndex = 0;

            foreach (Match m in _regex.Matches(unparsedString))
            {
                using (var data = new DataConnection())
                {
                    Guid id;
                    if (Guid.TryParse(m.Groups["id"].Value, out id))
                    {
                        var mediaFile = data.Get<IMediaFile>().FirstOrDefault(file => file.Id == id);
                        if (mediaFile != null)
                        {
                            builder.Append(unparsedString.Substring(startIndex, m.Index - startIndex));

                            var friendlyUrl = getFriendlyUrl(mediaFile);

                            if (!String.IsNullOrEmpty(m.Groups["extra_query"].Value))
                            {
                                var query = HttpUtility.ParseQueryString(HttpUtility.HtmlDecode(m.Groups["extra_query"].Value));
                                if (query.Count > 0)
                                {
                                    friendlyUrl += "?" + String.Join("&amp;", query.AllKeys.Where(k => !String.IsNullOrEmpty(k)).Select(k => k + "=" + query[k]));
                                }
                            }

                            builder.Append(friendlyUrl);

                            startIndex = m.Index + m.Length - 1;
                        }
                    }
                }                
            }

            builder.Append(unparsedString.Substring(startIndex, unparsedString.Length - startIndex));

            return builder.ToString();
        }

        private static string getFriendlyUrl(IMediaFile file)
        {
            if (AppSettings.UseFolderPathsForMediaUrls)
            {
                return String.Format("/media{0}/{1}", file.FolderPath, file.FileName);
            }
            else
            {
                return String.Format("/media/{0}/{1}", file.Id, file.FileName);
            }
        }
    }
}