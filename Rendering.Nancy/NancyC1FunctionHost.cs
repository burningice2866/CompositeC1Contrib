using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

using Composite.Functions;

using Nancy;
using Nancy.Bootstrapper;
using Nancy.IO;

namespace CompositeC1Contrib.Rendering.Nancy
{
    public class NancyC1FunctionHost
    {
        private static readonly INancyEngine Engine;

        static NancyC1FunctionHost()
        {
            var bootstrapper = GetBootstrapper();

            bootstrapper.Initialise();

            Engine = bootstrapper.GetEngine();
        }

        public C1NancyResponse ProcessRequest(HttpContextBase context, string path, ParameterList parameters)
        {
            var request = CreateNancyRequest(context, path);

            C1NancyResponse c1NancyResponse = null;
            Exception exception = null;

            Engine.HandleRequest(request,
                ctx => PreRequest(ctx, parameters),
                ctx =>
                {
                    c1NancyResponse = ResponseComplete(ctx);
                },
                exc =>
                {
                    exception = exc;
                },
                new CancellationToken());

            if (exception != null)
            {
                throw exception;
            }

            return c1NancyResponse;
        }

        private static NancyContext PreRequest(NancyContext ctx, ParameterList parameters)
        {
            ctx.Items["FunctionParameters"] = parameters;

            return ctx;
        }

        private static C1NancyResponse ResponseComplete(NancyContext ctx)
        {
            using (var ms = new MemoryStream())
            {
                ctx.Response.Contents(ms);

                ms.Seek(0, SeekOrigin.Begin);

                using (var sr = new StreamReader(ms))
                {
                    return new C1NancyResponse
                    {
                        StatusCode = ctx.Response.StatusCode,
                        Content = sr.ReadToEnd()
                    };
                }
            }
        }

        private static INancyBootstrapper GetBootstrapper()
        {
            return NancyBootstrapperLocator.Bootstrapper;
        }

        private static Request CreateNancyRequest(HttpContextBase context, string path)
        {
            var incomingHeaders = ToDictionary(context.Request.Headers);
            var expectedRequestLength = GetExpectedRequestLength(incomingHeaders);

            var request = context.Request;
            var url = request.Url;
            var basePath = request.ApplicationPath.TrimEnd('/');

            path = String.IsNullOrWhiteSpace(path) ? "/" : path;

            var nancyUrl = new Url
            {
                Scheme = url.Scheme,
                HostName = url.Host,
                Port = url.Port,
                BasePath = basePath,
                Path = path,
                Query = url.Query,
            };

            byte[] certificate = null;

            var clientCertificate = request.ClientCertificate;
            if (clientCertificate != null && clientCertificate.IsPresent && clientCertificate.Certificate.Length != 0)
            {
                certificate = clientCertificate.Certificate;
            }

            return new Request(
                request.HttpMethod.ToUpperInvariant(),
                nancyUrl,
                RequestStream.FromStream(request.InputStream, expectedRequestLength, true),
                incomingHeaders,
                request.UserHostAddress,
                certificate);
        }

        private static long GetExpectedRequestLength(IDictionary<string, IEnumerable<string>> incomingHeaders)
        {
            if (incomingHeaders == null)
            {
                return 0;
            }

            if (!incomingHeaders.ContainsKey("Content-Length"))
            {
                return 0;
            }

            var headerValue = incomingHeaders["Content-Length"].SingleOrDefault();
            if (headerValue == null)
            {
                return 0;
            }

            long contentLength;
            if (!long.TryParse(headerValue, NumberStyles.Any, CultureInfo.InvariantCulture, out contentLength))
            {
                return 0;
            }

            return contentLength;
        }

        public static IDictionary<string, IEnumerable<string>> ToDictionary(NameValueCollection source)
        {
            return source.AllKeys.ToDictionary(key => key, new Func<string, IEnumerable<string>>(source.GetValues));
        }
    }
}
