using System;
using System.Web;
using System.Web.Routing;

namespace CompositeC1Contrib.Rendering.Mvc.Routing
{
    public class RouteInfo
    {
        private readonly RouteCollection _routeCollection = new RouteCollection();

        public RouteInfo(RouteBase route)
        {
            _routeCollection.Add(route);
        }

        public RouteData GetRouteData(Uri uri)
        {
            return _routeCollection.GetRouteData(new InternalHttpContext(uri));
        }

        private class InternalHttpContext : HttpContextBase
        {
            private readonly HttpRequestBase _request;

            public InternalHttpContext(Uri uri)
            {
                _request = new InternalRequestContext(uri);
            }

            public override HttpRequestBase Request
            {
                get { return _request; }
            }
        }

        private class InternalRequestContext : HttpRequestBase
        {
            private readonly string _appRelativePath;
            private readonly string _pathInfo;

            public InternalRequestContext(Uri uri)
            {
                _pathInfo = String.Empty;
                _appRelativePath = uri.AbsolutePath;
            }

            public override string AppRelativeCurrentExecutionFilePath
            {
                get { return String.Concat("~", _appRelativePath); }
            }

            public override string PathInfo
            {
                get { return _pathInfo; }
            }
        }
    }
}
