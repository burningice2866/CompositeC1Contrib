using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;

using Composite.Core.IO;
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

        private static IOrderProcessor _orderProcessor;
        public static IOrderProcessor OrderProcessor
        {
            get
            {
                if (_orderProcessor == null)
                {
                    lock (Lock)
                    {
                        if (_orderProcessor == null)
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

                            try
                            {
                                _orderProcessor = Activator.CreateInstance(type) as IOrderProcessor;
                            }
                            catch (Exception e)
                            {
                                ECommerceLog.WriteLog("Error instantiating orderprocessor", e);
                            }
                        }
                    }
                }

                return _orderProcessor;
            }
        }

        public static IReadOnlyDictionary<string, PaymentProvider> Providers
        {
            get
            {
                return Section.Providers.Cast<ProviderSettings>()
                    .Select(p => Section.GetProviderInstance(p.Name))
                    .Where(p => p != null)
                    .ToDictionary(p => p.Name);
            }
        }

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

        public static IShopOrder CreateNewOrder(decimal totalAmount)
        {
            return CreateNewOrder(totalAmount, Section.DefaultCurrency, null);
        }

        public static IShopOrder CreateNewOrder(decimal totalAmount, Currency currency)
        {
            return CreateNewOrder(totalAmount, currency, null);
        }

        public static IShopOrder CreateNewOrder(decimal totalAmount, string customData)
        {
            return CreateNewOrder(totalAmount, Section.DefaultCurrency, customData);
        }

        public static IShopOrder CreateNewOrder(decimal totalAmount, Currency currency, string customData)
        {
            var id = GenerateNextOrderNumber();

            return CreateNewOrder(id, totalAmount, currency, customData);
        }

        public static IShopOrder CreateNewOrder(string orderId, decimal totalAmount, string customData)
        {
            return CreateNewOrder(orderId, totalAmount, Section.DefaultCurrency, customData);
        }

        public static IShopOrder CreateNewOrder(string orderId, decimal totalAmount, Currency currency, string customData)
        {
            using (var data = new DataConnection())
            {
                var order = data.CreateNew<IShopOrder>();

                order.Id = orderId;
                order.CreatedOn = DateTime.UtcNow;
                order.OrderTotal = totalAmount;
                order.Currency = currency.ToString();
                order.CustomData = customData;

                order = data.Add(order);

                order.WriteLog("created");

                return order;
            }
        }

        public static string GenerateNextOrderNumber()
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
                sOrderId = OrderNumberPrefix + sOrderId;
            }

            if (IsTestMode)
            {
                var hashMachineName = Environment.MachineName.GetHashCode().ToString(CultureInfo.InvariantCulture);
                if (hashMachineName.StartsWith("-"))
                {
                    hashMachineName = hashMachineName.Remove(0, 1);
                }

                var maxMashineNameLength = 20 - sOrderId.Length - 4;
                if (hashMachineName.Length > maxMashineNameLength)
                {
                    hashMachineName = hashMachineName.Substring(0, maxMashineNameLength);
                }

                sOrderId = String.Format("TEST{0}{1}", hashMachineName, sOrderId);
            }

            sOrderId = sOrderId.Trim();

            return sOrderId;
        }
    }
}
