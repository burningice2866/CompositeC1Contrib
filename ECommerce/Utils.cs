using System;
using System.IO;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public class Utils
    {
        public static void WriteLog(IShopOrder order, string message)
        {
            string logFile;

            if (order == null)
            {
                logFile = Path.Combine(ECommerce.RootPath, "log.txt");
            }
            else
            {
                logFile = Path.Combine(ECommerce.RootPath, String.Format("log.{0}.txt", order.Id));
            }

            lock (FileUtils.SyncRoot)
            {
                using (var writer = FileUtils.GetOrCreateFile(logFile))
                {
                    writer.Write(DateTime.Now);
                    writer.Write("\t");
                    writer.Write(message);
                    writer.Write(Environment.NewLine);
                }
            }
        }

        public static void PostProcessOrder(IShopOrder order, IOrderProcessor processor, DataConnection data)
        {
            WriteLog(order, "Postprocessing order");

            try
            {
                processor.PostProcessOrder(order);

                order.PostProcessed = true;

                data.Update(order);

                WriteLog(order, "Order postprocessed");
            }
            catch (Exception ex)
            {
                WriteLog(order, "Unhandled error when postprocessing order, Exception: " + ex);
            }
        }
    }
}
