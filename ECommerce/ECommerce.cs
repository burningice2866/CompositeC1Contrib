using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Configuration;
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public static class ECommerce
    {
        private static readonly object Lock = new object();

        public static readonly string RootPath = HostingEnvironment.MapPath("~/App_Data/ECommerce");
        private static readonly string LastUsedOrderIdFile = Path.Combine(RootPath, "LastUsedOrderId.txt");

        private static readonly ECommerceSection Section = ECommerceSection.GetSection();

        public static int MinimumOrderNumberLength
        {
            get { return Section.MinimumOrderNumberLength; }
        }

        public static string OrderNumberPrefix
        {
            get { return Section.OrderNumberPrefix; }
        }

        public static bool IsTestMode
        {
            get { return Section.TestMode; }
        }

        public static PaymentProvider DefaultProvider
        {
            get
            {
                var settings = Section.Providers.Cast<ProviderSettings>().Single(p => p.Name == Section.DefaultProvider);
                var type = Type.GetType(settings.Type);

                return (PaymentProvider)ProvidersHelper.InstantiateProvider(settings, type);
            }
        }

        public static IOrderProcessor OrderProcessor
        {
            get
            {
                if (String.IsNullOrEmpty(Section.OrderProcessor))
                {
                    return null;
                }

                var type = Type.GetType(Section.OrderProcessor);
                if (type == null)
                {
                    return null;
                }

                return Activator.CreateInstance(type) as IOrderProcessor;
            }
        }

        public static IShopOrder CreateNewOrder(decimal totalAmount)
        {
            return CreateNewOrder(totalAmount, null);
        }

        public static IShopOrder CreateNewOrder(decimal totalAmount, string customData)
        {
            var id = GenerateNextOrderNumber();

            return CreateNewOrder(id, totalAmount, customData);
        }

        public static IShopOrder CreateNewOrder(string orderId, decimal totalAmount, string customData)
        {
            using (var data = new DataConnection())
            {
                var order = data.CreateNew<IShopOrder>();

                order.Id = orderId;
                order.CreatedOn = DateTime.Now;
                order.OrderTotal = totalAmount;
                order.CustomData = customData;

                data.Add(order);

                Utils.WriteLog(order, "New order created");

                return order;
            }
        }

        public static string GenerateNextOrderNumber()
        {
            int orderId;

            lock (Lock)
            {
                var content = File.ReadAllText(LastUsedOrderIdFile);
                int.TryParse(content, out orderId);

                orderId = orderId + 1;

                File.WriteAllText(LastUsedOrderIdFile, orderId.ToString(CultureInfo.InvariantCulture));
            }

            string sOrderId;
            if (MinimumOrderNumberLength > 0)
            {
                var format = "D" + MinimumOrderNumberLength;

                sOrderId = orderId.ToString(format, CultureInfo.InvariantCulture);
            }
            else
            {
                sOrderId = orderId.ToString(CultureInfo.InvariantCulture);
            }

            if (!String.IsNullOrEmpty(OrderNumberPrefix))
            {
                sOrderId = OrderNumberPrefix + orderId;
            }

            if (IsTestMode)
            {
                var hashMachineName = Environment.MachineName.GetHashCode().ToString(CultureInfo.InvariantCulture);
                if (hashMachineName.StartsWith("-"))
                {
                    hashMachineName = hashMachineName.Remove(0, 1);
                }

                if (hashMachineName.Length > 8)
                {
                    hashMachineName = hashMachineName.Substring(0, 8);
                }

                sOrderId = String.Format("TEST{0}{1}", hashMachineName, sOrderId);
            }

            sOrderId = sOrderId.Trim();

            return sOrderId;
        }
    }
}
