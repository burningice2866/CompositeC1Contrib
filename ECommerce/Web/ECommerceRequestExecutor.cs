using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Configuration;
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce.Web
{
    public class ECommerceRequestExecutor
    {
        private static readonly ECommerceSection Config = ECommerceSection.GetSection();
        private static readonly PaymentProvider Provider = ECommerce.DefaultProvider;
        private static readonly IOrderProcessor OrderProcessor = ECommerce.OrderProcessor;

        private readonly HttpContextBase _context;

        public ECommerceRequestExecutor(HttpContextBase context)
        {
            _context = context;
        }

        public Task HandleDefault()
        {
            return SyncAction(() =>
            {
                var orderId = _context.Request.QueryString["orderid"];

                Utils.WriteLog("Default request recieved on orderid " + orderId);

                using (var data = new DataConnection())
                {
                    var order = data.Get<IShopOrder>().SingleOrDefault(o => o.Id == orderId);
                    if (order == null)
                    {
                        Utils.WriteLog("No order with id " + orderId);

                        _context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                        return;
                    }

                    Utils.WriteLog(order, "paymentwindow requested");

                    if (order.PaymentStatus == (int)PaymentStatus.Authorized)
                    {
                        Utils.WriteLog(order, "debug", "Order has already been authorized");

                        _context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                        return;
                    }

                    var window = Provider.GeneratePaymentWindow(order, _context.Request.Url);

                    HtmlContent(window);
                }
            });
        }

        public Task HandleCancel()
        {
            return SyncAction(() =>
            {
                Utils.WriteLog("Cancel request recieved, redirecting to main page");

                var pageUrl = GetPageUrl(Config.MainPageId) + "?reason=cancel";

                RedirectOrIFrame(pageUrl);
            });
        }

        public async Task HandleCallback()
        {
            Utils.WriteLog("Callback request recieved");

            var order = await Provider.HandleCallbackAsync(_context);
            if (order == null)
            {
                Utils.WriteLog("Callback failed");

                _context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return;
            }

            Utils.WriteLog(order, "callback succeeded");

            ECommerceBackgroundProcess.ProcessOrdersNow();
        }

        public async Task HandleContinue()
        {
            var orderId = _context.Request.QueryString["orderid"];

            Utils.WriteLog("Continue request recieved on orderid " + orderId);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().SingleOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    Utils.WriteLog("No order with id " + orderId);

                    _context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                    return;
                }

                Utils.WriteLog(order, "continue requested");

                var hasContinued = Utils.GetLog(order).Any(l => l.Title == "continue succeeded");
                if (hasContinued)
                {
                    Utils.WriteLog(order, "debug", "Continue has already succeeded");

                    _context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return;
                }

                var isAuthorized = await Provider.IsPaymentAuthorizedAsync(order);
                if (!isAuthorized)
                {
                    Utils.WriteLog(order, "debug", "Payment isn't authorized");

                    _context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return;
                }

                Receipt(order);

                Utils.WriteLog(order, "continue succeeded");
            }
        }

        private void Receipt(IShopOrder order)
        {
            if (OrderProcessor != null)
            {
                var pageUrlOverride = OrderProcessor.HandleContinue(_context, order);
                if (!String.IsNullOrEmpty(pageUrlOverride))
                {
                    RedirectOrIFrame(pageUrlOverride);

                    return;
                }
            }

            var pageUrl = GetPageUrl(Config.ReceiptPageId);
            if (String.IsNullOrEmpty(pageUrl))
            {
                pageUrl = GetPageUrl(Config.MainPageId);
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

        private static string GetPageUrl(string id)
        {
            string pathInfo = null;

            if (id.Contains("/"))
            {
                var split = id.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                id = split[0];
                pathInfo = split[1];
            }

            var node = SiteMap.Provider.FindSiteMapNodeFromKey(id);
            if (node == null)
            {
                return null;
            }

            var url = node.Url;

            if (!String.IsNullOrEmpty(pathInfo))
            {
                url += "/" + pathInfo;
            }

            return url;
        }

        private static Task SyncAction(Action action)
        {
            var task = new Task(action, TaskCreationOptions.HideScheduler);

            task.RunSynchronously();

            return task;
        }
    }
}
