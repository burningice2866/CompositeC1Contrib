using System;
using System.Globalization;
using System.IO;
using System.Web;

using Composite.Core.IO;

using CompositeC1Contrib.ECommerce.Configuration;
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public class DefaultOrderProcessor : IOrderProcessor
    {
        private static readonly object Lock = new object();
        private static readonly string LastUsedOrderIdFile = Path.Combine(ECommerce.RootPath, "LastUsedOrderId.txt");

        public virtual string GenerateNextOrderId(OrderCreationSettings settings)
        {
            int orderId;

            lock (Lock)
            {
                var content = C1File.ReadAllText(LastUsedOrderIdFile);
                int.TryParse(content, out orderId);

                orderId = orderId + 1;

                C1File.WriteAllText(LastUsedOrderIdFile, orderId.ToString(CultureInfo.InvariantCulture));
            }

            string sOrderId;
            if (settings.MinimumOrderIdLength > 0)
            {
                var format = "D" + settings.MinimumOrderIdLength;

                sOrderId = orderId.ToString(format, CultureInfo.InvariantCulture);
            }
            else
            {
                sOrderId = orderId.ToString(CultureInfo.InvariantCulture);
            }

            if (!String.IsNullOrEmpty(settings.OrderIdPrefix))
            {
                sOrderId = settings.OrderIdPrefix + sOrderId;
            }

            if (ECommerce.IsTestMode)
            {
                var hashMachineName = Environment.MachineName.GetHashCode().ToString(CultureInfo.InvariantCulture);
                if (hashMachineName.StartsWith("-"))
                {
                    hashMachineName = hashMachineName.Remove(0, 1);
                }

                var maxMachineNameLength = 20 - 6 - sOrderId.Length;
                if (hashMachineName.Length > maxMachineNameLength)
                {
                    hashMachineName = hashMachineName.Substring(0, maxMachineNameLength);
                }

                sOrderId = $"TEST-{hashMachineName}-{sOrderId}";
            }

            sOrderId = sOrderId.Trim();

            return sOrderId;
        }

        public virtual bool HandleCallback(HttpContextBase context, IShopOrder order)
        {
            return false;
        }

        public virtual string HandleContinue(HttpContextBase context, IShopOrder order)
        {
            return null;
        }

        public virtual string HandleCancel(HttpContextBase context)
        {
            var config = ECommerceSection.GetSection();

            var pageUrl = GetPageUrl(config.MainPageId);
            if (String.IsNullOrEmpty(pageUrl))
            {
                pageUrl = "/";
            }

            return pageUrl + "?reason=cancel";
        }

        public virtual void PostProcessOrder(IShopOrder order) { }

        public static string GetPageUrl(string id)
        {
            var pathInfo = String.Empty;

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
