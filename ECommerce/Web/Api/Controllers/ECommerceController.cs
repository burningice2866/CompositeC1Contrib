using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Configuration;
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce.Web
{
    public class ECommerceController : ApiController
    {
        private static readonly ECommerceSection Config = ECommerceSection.GetSection();
        private static readonly PaymentProvider Provider = ECommerce.DefaultProvider;

        [HttpGet]
        public IHttpActionResult Default()
        {
            var qs = GetQueryString();
            var orderId = qs["orderid"];

            Utils.WriteLog(null, "Default request recieved on orderid " + orderId);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().SingleOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    Utils.WriteLog(null, "No order with number " + orderId);

                    return NotFound();
                }

                if (Provider.IsAuthorizedRequest(qs))
                {
                    Utils.WriteLog(order, "Request is authorized, redirecting to reciept page");

                    var recieptResult = Reciept(order);
                    if (recieptResult != null)
                    {
                        return recieptResult;
                    }
                }

                if (!String.IsNullOrEmpty(order.AuthorizationXml))
                {
                    Utils.WriteLog(order, "Order has already been handled by payment gateway");

                    return StatusCode(HttpStatusCode.BadRequest);
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

            var confirmUrl = GetPageUrl(Config.MainPageId);
            if (!String.IsNullOrEmpty(confirmUrl))
            {
                if (Config.UseIFrame)
                {
                    return HtmlContent("<script>parent.location.href = '" + confirmUrl + "';</script>");
                }

                return Redirect(confirmUrl);
            }

            return StatusCode(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        [ActionName("callback")]
        public async Task<IHttpActionResult> Callback()
        {
            Utils.WriteLog(null, "Callback request recieved");

            var order = await Provider.HandleCallbackAsync(ActionContext.Request);

            Utils.WriteLog(order, "Authorized with the following transactionid " + order.AuthorizationTransactionId);

            return Ok();
        }

        [HttpGet]
        [ActionName("continue")]
        public async Task<IHttpActionResult> Continue()
        {
            var qs = GetQueryString();
            var orderId = qs["orderid"];

            Utils.WriteLog(null, "Continue request recieved on orderid " + orderId);

            var order = await GetAuthorizedOrder(orderId);
            if (order == null)
            {
                Utils.WriteLog(null, "No authorized order with number " + orderId);

                return NotFound();
            }

            IHttpActionResult orderProcessorResult = null;

            var orderProcessor = ECommerce.OrderProcessor;
            if (orderProcessor != null)
            {
                orderProcessorResult = orderProcessor.HandleHandlerContinue(order, ActionContext.Request, ActionContext.Response);
            }

            ECommerceWorker.ProcessOrdersNow();

            if (orderProcessorResult != null)
            {
                return orderProcessorResult;
            }

            var recieptResult = Reciept(order);
            if (recieptResult != null)
            {
                return recieptResult;
            }

            return StatusCode(HttpStatusCode.BadRequest);
        }

        private IDictionary<string, string> GetQueryString()
        {
            return Request.GetQueryNameValuePairs().ToDictionary(el => el.Key, el => el.Value);
        }

        private IHttpActionResult HtmlContent(string content)
        {
            var message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(content, Encoding.UTF8, "text/html");

            return ResponseMessage(message);
        }

        private IHttpActionResult Reciept(IShopOrder order)
        {
            var recieptUrl = GetPageUrl(Config.RecieptPageId);
            if (String.IsNullOrEmpty(recieptUrl))
            {
                return null;
            }

            recieptUrl = recieptUrl + "?orderid=" + order.Id;

            if (Config.UseIFrame)
            {
                return HtmlContent("<script>parent.location.href = '" + recieptUrl + "';</script>");
            }

            return Redirect(recieptUrl);
        }

        private Task<IShopOrder> GetAuthorizedOrder(string orderId)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var data = new DataConnection())
                {
                    IShopOrder order = null;
                    var start = DateTime.UtcNow;

                    while (order == null && ((DateTime.UtcNow - start) < TimeSpan.FromSeconds(2)))
                    {
                        Thread.Sleep(100);

                        order = data.Get<IShopOrder>().SingleOrDefault(o => o.Id == orderId && o.PaymentStatus == (int)PaymentStatus.Authorized);
                    }

                    return order;
                }
            });
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
