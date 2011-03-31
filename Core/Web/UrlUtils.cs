using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompositeC1Contrib.Web
{
    public static class UrlUtils
    {
        private static readonly IDictionary<char, char> allowedChars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }.ToDictionary(t => t);
        private static readonly IDictionary<char, string> replaceChars = new Dictionary<char, string>();

        static UrlUtils()
        {
            replaceChars['á'] = "a";
            replaceChars['à'] = "a";
            replaceChars['â'] = "a";
            replaceChars['ä'] = "a";
            replaceChars['ã'] = "a";

            replaceChars['ð'] = "d";

            replaceChars['é'] = "e";
            replaceChars['è'] = "e";
            replaceChars['ê'] = "e";
            replaceChars['ë'] = "e";

            replaceChars['í'] = "i";
            replaceChars['ì'] = "i";
            replaceChars['î'] = "i";
            replaceChars['ï'] = "i";

            replaceChars['ñ'] = "n";

            replaceChars['ó'] = "o";
            replaceChars['ò'] = "o";
            replaceChars['ô'] = "o";
            replaceChars['ö'] = "o";
            replaceChars['õ'] = "o";

            replaceChars['ú'] = "u";
            replaceChars['ù'] = "u";
            replaceChars['û'] = "u";
            replaceChars['ü'] = "u";

            replaceChars['ý'] = "y";
            replaceChars['ÿ'] = "y";

            replaceChars['æ'] = "ae";
            replaceChars['ø'] = "oe";
            replaceChars['å'] = "aa";

            replaceChars['-'] = "_";
            replaceChars[' '] = "_";
        }

        public static bool IsDefaultDocumentUrl(string url)
        {
            return url == "/"
            || url.StartsWith("/?")
            || url.StartsWith("/default.aspx", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetCleanUrl(string url)
        {
            var sb = new StringBuilder();
            var split = url.ToLower().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < split.Length; i++)
            {
                if ((i == 0 && url.StartsWith("/")) || i > 0)
                {
                    sb.Append("/");
                }

                char[] arr = split[i].Trim().ToCharArray();
                for (int j = 0; j < arr.Length; j++)
                {
                    char c = arr[j];

                    if (allowedChars.ContainsKey(c) || Char.IsDigit(c))
                    {
                        sb.Append(c);
                    }

                    if (replaceChars.ContainsKey(c))
                    {
                        string replacement = replaceChars[c];
                        sb.Append(replacement);
                    }
                }
            }

            sb.Replace("__", "_");

            return sb.ToString();
        }
    }
}
