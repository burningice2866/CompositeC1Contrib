using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;

namespace CompositeC1Contrib.Web
{
    public class GlobalizationModule : IHttpModule
    {
        private static IDictionary<string, CultureInfo> _ci = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToDictionary(c => c.Name);

        public HttpContext Context
        {
            get { return HttpContext.Current; }
        }

        private void Resolve(object sender, EventArgs e)
        {
            var ci = CultureInfo.CurrentCulture;

            ci = ResolveCultureByPage();

            if (ci != null)
            {
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = ci;
            }
        }

        private CultureInfo ResolveCultureByPage()
        {
            var request = Context.Request;
            string culture = null;

            try
            {
                var path = request.Url.AbsolutePath.Substring(1);
                if (path.Contains("/"))
                {
                    culture = path.Substring(0, path.IndexOf("/"));
                }
                else
                {
                    culture = path;
                }
            }
            catch (ArgumentOutOfRangeException) { }

            CultureInfo ci = null;
            if (_ci.TryGetValue(culture, out ci))
            {
                string rewriteUrl = request.RawUrl.Replace("/" + culture, String.Empty);
                Context.RewritePath(rewriteUrl);
            }

            return ci;
        }

        void IHttpModule.Init(HttpApplication app)
        {
            app.PostAuthorizeRequest += new EventHandler(Resolve);
        }

        void IHttpModule.Dispose() { }
    }
}
