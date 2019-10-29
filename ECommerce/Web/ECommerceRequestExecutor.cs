using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Configuration;
using CompositeC1Contrib.ECommerce.Data.Types;
using CompositeC1Contrib.ECommerce.PaymentProviders;

namespace CompositeC1Contrib.ECommerce.Web
{
    public class ECommerceRequestExecutor
    {
        private static readonly ECommerceSection Config = ECommerceSection.GetSection();
        private static readonly IOrderProcessor OrderProcessor = ECommerce.OrderProcessor;
        private static readonly IReadOnlyDictionary<string, PaymentProviderBase> Providers = ECommerce.Providers;

        private readonly HttpContextBase _context;

        public ECommerceRequestExecutor(HttpContextBase context)
        {
            _context = context;
        }

        public Task HandleDefault()
        {
            var orderId = _context.Request.QueryString["orderid"];
            if (String.IsNullOrEmpty(orderId))
            {
                orderId = _context.Request.Form["orderoid"];
            }

            ECommerceLog.WriteLog("Default request received on order id " + orderId);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().SingleOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    ECommerceLog.WriteLog("No order with id " + orderId);

                    _context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                    return Task.FromResult(0);
                }

                order.WriteLog("Payment window requested");

                if (order.PaymentStatus == (int)PaymentStatus.Authorized)
                {
                    order.WriteLog("debug", "Order has already been authorized");

                    _context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return Task.FromResult(0);
                }

                var provider = ResolvePaymentProvider(order.Id);
                var window = provider.GeneratePaymentWindow(order, _context.Request.Url);

                HtmlContent(window);
            }

            return Task.FromResult(0);
        }

        public Task HandleCancel()
        {
            ECommerceLog.WriteLog("Cancel request received");

            var pageUrl = OrderProcessor.HandleCancel(_context);

            RedirectOrIFrame(pageUrl);

            return Task.FromResult(0);
        }

        public async Task HandleCallback()
        {
            ECommerceLog.WriteLog("Callback request received");

            var orderId = await ResolveOrderIdFromRequestAsync(_context.Request);
            var provider = ResolvePaymentProvider(orderId);

            var order = await provider.HandleCallbackAsync(_context);
            if (order == null)
            {
                ECommerceLog.WriteLog("Callback failed");

                _context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return;
            }

            order.WriteLog("callback succeeded");

            ECommerceBackgroundProcess.ProcessOrdersNow();
        }

        public async Task HandleContinue()
        {
            var orderId = _context.Request.QueryString["orderid"];
            if (String.IsNullOrEmpty(orderId))
            {
                orderId = _context.Request.Form["orderid"];
            }

            ECommerceLog.WriteLog("Continue request received on order id " + orderId);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().SingleOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    ECommerceLog.WriteLog("No order with id " + orderId);

                    _context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                    return;
                }

                order.WriteLog("continue requested");

                var hasContinued = order.GetLog().Any(l => l.Title == "continue succeeded");
                if (hasContinued)
                {
                    order.WriteLog("debug", "Continue has already succeeded");

                    _context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return;
                }

                var isAuthorized = (PaymentStatus)order.PaymentStatus == PaymentStatus.Authorized;
                if (!isAuthorized)
                {
                    var provider = ResolvePaymentProvider(order.Id);

                    isAuthorized = await provider.IsPaymentAuthorizedAsync(order);
                }

                if (!isAuthorized)
                {
                    order.WriteLog("debug", "Payment isn't authorized");

                    _context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return;
                }

                Receipt(order);

                order.WriteLog("continue succeeded");
            }
        }

        private static async Task<string> ResolveOrderIdFromRequestAsync(HttpRequestBase request)
        {
            foreach (var provider in Providers.Values)
            {
                var orderId = await provider.ResolveOrderIdFromRequestAsync(request);
                if (!String.IsNullOrEmpty(orderId))
                {
                    return orderId;
                }
            }

            throw new InvalidOperationException("Order id couldn't be resolved");
        }

        private static PaymentProviderBase ResolvePaymentProvider(string orderId)
        {
            using (var data = new DataConnection())
            {
                var paymentRequest = data.Get<IPaymentRequest>().SingleOrDefault(r => r.ShopOrderId == orderId);
                if (paymentRequest == null)
                {
                    throw new InvalidOperationException($"There is no payment request for order '{orderId}'");
                }

                return ECommerce.Providers[paymentRequest.ProviderName];
            }
        }

        private void Receipt(IShopOrder order)
        {
            var pageUrlOverride = OrderProcessor.HandleContinue(_context, order);
            if (!String.IsNullOrEmpty(pageUrlOverride))
            {
                RedirectOrIFrame(pageUrlOverride);

                return;
            }

            var pageUrl = DefaultOrderProcessor.GetPageUrl(Config.ReceiptPageId);
            if (String.IsNullOrEmpty(pageUrl))
            {
                pageUrl = DefaultOrderProcessor.GetPageUrl(Config.MainPageId);
                if (String.IsNullOrEmpty(pageUrl))
                {
                    pageUrl = "/";
                }
            }

            pageUrl = pageUrl + "?orderid=" + order.Id;

            RedirectOrIFrame(pageUrl);
        }

        private void RedirectOrIFrame(string pageUrl)
        {
            var uri = new Uri(pageUrl, UriKind.Relative);

            if (Config.UseIFrame)
            {
                HtmlContent("<script>parent.location.href = '" + uri + "';</script>");
            }
            else
            {
                Redirect(uri);
            }
        }

        private void Redirect(Uri uri)
        {
            _context.Response.RedirectLocation = uri.ToString();
            _context.Response.StatusCode = (int)HttpStatusCode.Redirect;
        }

        private void HtmlContent(string content)
        {
            _context.Response.Write(content);
        }
    }
}
