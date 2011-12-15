using System;
using System.Web;

namespace CompositeC1Contrib.UserControlFunctions.FunctionProvider
{
    public class PopulateUserControlParametersContext : IDisposable
    {
        HttpContext _oldHttpContext;
        bool _restoreOldHttpContext = false;

        public PopulateUserControlParametersContext(bool forceSettingContextToNull = false)
        {
            if (forceSettingContextToNull)
            {
                _oldHttpContext = HttpContext.Current;
                HttpContext.Current = null;
                _restoreOldHttpContext = true;
            }
            else
            {
                try
                {
                    var ctx = HttpContext.Current;
                    if (ctx != null)
                    {
                        var response = HttpContext.Current.Response;
                    }
                }
                catch (HttpException)
                {
                    _oldHttpContext = HttpContext.Current;
                    HttpContext.Current = null;
                    _restoreOldHttpContext = true;
                }
            }
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_restoreOldHttpContext)
                {
                    HttpContext.Current = _oldHttpContext;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~PopulateUserControlParametersContext()
        {
            Dispose(false);
        }
    }
}
