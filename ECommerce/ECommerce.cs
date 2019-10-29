﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using Composite.Core.IO;
using Composite.Data;

using CompositeC1Contrib.ECommerce.Configuration;
using CompositeC1Contrib.ECommerce.Data.Types;
using CompositeC1Contrib.ECommerce.PaymentProviders;

namespace CompositeC1Contrib.ECommerce
{
    public static class ECommerce
    {
        private static readonly object Lock = new object();

        public static readonly string RootPath = PathUtil.Resolve("~/App_Data/ECommerce");

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
                                return new DefaultOrderProcessor();
                            }

                            var type = Type.GetType(Section.OrderProcessor);
                            if (type == null)
                            {
                                ECommerceLog.WriteLog($"Unknown orderprocessor type '{Section.OrderProcessor}'");

                                return new DefaultOrderProcessor();
                            }

                            try
                            {
                                _orderProcessor = Activator.CreateInstance(type) as IOrderProcessor;
                            }
                            catch (Exception e)
                            {
                                ECommerceLog.WriteLog("Error instantiating orderprocessor", e);

                                return new DefaultOrderProcessor();
                            }
                        }
                    }
                }

                return _orderProcessor;
            }
        }

        public static IReadOnlyDictionary<string, PaymentProviderBase> Providers
        {
            get
            {
                return Section.Providers.Cast<ProviderSettings>()
                    .Select(p => Section.GetProviderInstance(p.Name))
                    .Where(p => p != null)
                    .ToDictionary(p => p.Name);
            }
        }

        public static bool IsTestMode => Section.TestMode;

        public static IShopOrder CreateNewOrder(decimal totalAmount)
        {
            return CreateNewOrder(new OrderCreationSettings
            {
                TotalAmount = totalAmount
            });
        }

        public static IShopOrder CreateNewOrder(decimal totalAmount, Currency currency)
        {
            return CreateNewOrder(new OrderCreationSettings
            {
                TotalAmount = totalAmount,
                Currency = currency
            });
        }

        public static IShopOrder CreateNewOrder(decimal totalAmount, Currency currency, string customData)
        {
            return CreateNewOrder(new OrderCreationSettings
            {
                TotalAmount = totalAmount,
                Currency = currency,
                CustomData = customData
            });
        }

        public static IShopOrder CreateNewOrder(decimal totalAmount, string customData)
        {
            return CreateNewOrder(new OrderCreationSettings
            {
                TotalAmount = totalAmount,
                CustomData = customData
            });
        }

        public static IShopOrder CreateNewOrder(OrderCreationSettings settings)
        {
            if (OrderProcessor == null)
            {
                throw new InvalidOperationException("No order processor present, can't generate a new orderid");
            }

            var orderId = OrderProcessor.GenerateNextOrderId(settings);

            using (var data = new DataConnection())
            {
                var order = data.CreateNew<IShopOrder>();

                order.Id = orderId;
                order.CreatedOn = DateTime.UtcNow;
                order.OrderTotal = settings.TotalAmount;
                order.Currency = settings.Currency.ToString();
                order.CustomData = settings.CustomData;

                order = data.Add(order);

                order.WriteLog("created");

                return order;
            }
        }
    }
}
