using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Composite.Data;
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public static class Utils
    {
        static readonly string LogFile = Path.Combine(ECommerce.RootPath, "log.txt");

        public static void WriteLog(string message)
        {
            WriteLog(message, null);
        }

        public static void WriteLog(string message, Exception exc)
        {
            lock (FileUtils.SyncRoot)
            {
                using (var writer = FileUtils.GetOrCreateFile(LogFile))
                {
                    if (exc != null)
                    {
                        message += ", Exception: " + exc;
                    }

                    writer.Write(DateTime.Now);
                    writer.Write("\t");
                    writer.Write(message);
                    writer.Write(Environment.NewLine);
                }
            }
        }

        public static IShopOrderLog WriteLog(IShopOrder order, string logTitle)
        {
            return WriteLog(order, logTitle, null);
        }

        public static IShopOrderLog WriteLog(IShopOrder order, string logTitle, string logData)
        {
            using (var data = new DataConnection())
            {
                var entry = data.CreateNew<IShopOrderLog>();

                entry.Id = Guid.NewGuid();
                entry.ShopOrderId = order.Id;
                entry.Timestamp = DateTime.UtcNow;
                entry.Title = logTitle;
                entry.Data = logData;

                return data.Add(entry);
            }
        }

        public static IEnumerable<IShopOrderLog> GetLog(IShopOrder order)
        {
            using (var data = new DataConnection())
            {
                return data.Get<IShopOrderLog>().Where(l => l.ShopOrderId == order.Id).ToList();
            }
        }

        public static void PostProcessOrder(IShopOrder order, IOrderProcessor processor, DataConnection data)
        {
            WriteLog(order, "postprocessing");

            try
            {
                processor.PostProcessOrder(order);

                order.PostProcessed = true;

                data.Update(order);

                WriteLog(order, "postprocessed");
            }
            catch (Exception ex)
            {
                WriteLog(order, "postprocessing error", ex.ToString());
            }
        }
    }
}
