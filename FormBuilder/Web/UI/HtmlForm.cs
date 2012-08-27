using System;

using CompositeC1Contrib.RazorFunctions;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public class HtmlForm : IDisposable
    {
        private CompositeC1WebPage _page;
        private bool _disposed;

        public HtmlForm(CompositeC1WebPage page)
        {
            _page = page;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _page.WriteLiteral("</form>");

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public void EndForm()
        {
            Dispose(true);
        }
    }
}
