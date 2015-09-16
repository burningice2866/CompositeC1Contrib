using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Configuration;
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce.Web.Api.Controllers
{
    public class ECommerceController : ApiController
    {
        private static readonly ECommerceSection Config = ECommerceSection.GetSection();
        private static readonly PaymentProvider Provider = ECommerce.DefaultProvider;
        private static readonly IOrderProcessor OrderProcessor = ECommerce.OrderProcessor;

        [HttpGet]
        public IHttpActionResult Default([FromUri]string orderId)
        {
            Utils.WriteLog(null, "Default request recieved on orderid " + orderId);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().SingleOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    Utils.WriteLog(null, "No order with id " + orderId);

                    return NotFound();
                }

                if (order.PaymentStatus == (int)PaymentStatus.Authorized)
                {
                    Utils.WriteLog(order, "Order has already been authorized");

                    return BadRequest();
                }

                Utils.WriteLog(order, "Generating payment window");

                var window = Provider.GeneratePaymentWindow(order, Request.RequestUri);

                return HtmlContent(window);
            }
        }

        [HttpGet]
        [ActionName("cancel")]
        public IHttpActionResult Cancel()
        {
            Utils.WriteLog(null, "Cancel request recieved, redirecting to main page");

            var pageUrl = GetPageUrl(Config.MainPageId) + "?reason=cancel";

            return RedirectOrIFrame(pageUrl);
        }

        [HttpPost]
        [ActionName("callback")]
        public async Task<IHttpActionResult> Callback()
        {
            Utils.WriteLog(null, "Callback request recieved");

            var order = await Provider.HandleCallbackAsync(Request);
            if (order == null)
            {
                Utils.WriteLog(null, "Callback failed");

                return BadRequest();
            }

            Utils.WriteLog(order, "Authorized with the following transactionid " + order.AuthorizationTransactionId);

            ECommerceWorker.ProcessOrdersNow();

            return Ok();
        }

        [HttpGet]
        [ActionName("continue")]
        public async Task<IHttpActionResult> Continue()
        {
            var qs = GetQueryString();
            var orderId = qs["orderid"];

            Utils.WriteLog(null, "Continue request recieved on orderid " + orderId);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().SingleOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    Utils.WriteLog(null, "No order with id " + orderId);

                    return NotFound();
                }

                if (order.PostProcessed)
                {
                    Utils.WriteLog(order, "Order has already been post processed");

                    return BadRequest();
                }

                var isAuthorized = await Provider.IsPaymentAuthorizedAsync(order);
                if (!isAuthorized)
                {
                    Utils.WriteLog(order, "Payment isn't authorized");

                    return BadRequest();
                }

                return Receipt(order);
            }
        }

        private IHttpActionResult Receipt(IShopOrder order)
        {
            if (OrderProcessor != null)
            {
                var receipt = OrderProcessor.Receipt(order, Request);
                if (receipt != null)
                {
                    return receipt;
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

            return RedirectOrIFrame(pageUrl);
        }

        private IHttpActionResult RedirectOrIFrame(string pageUrl)
        {
            var uri = new Uri(pageUrl, UriKind.Relative);

            return Config.UseIFrame ? HtmlContent("<script>parent.location.href = '" + uri + "';</script>") : Redirect(uri);
        }

        private IHttpActionResult HtmlContent(string content)
        {
            var message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "text/html")
            };

            return ResponseMessage(message);
        }

        private IDictionary<string, string> GetQueryString()
        {
            return Request.GetQueryNameValuePairs().ToDictionary(el => el.Key, el => el.Value);
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
    }
}
