using System;
using System.IO;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public class Utils
    {
        public static void WriteLog(string message, Exception exc)
        {
            WriteLog(null, message, exc);
        }

        public static void WriteLog(string message)
        {
            WriteLog(null, message);
        }

        public static void WriteLog(IShopOrder order, string message)
        {
            WriteLog(order, message, null);
        }

        public static void WriteLog(IShopOrder order, string message, Exception exc)
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

        public static void PostProcessOrder(IShopOrder order, IOrderProcessor processor)
        {
            WriteLog(order, "Postprocessing order");

            try
            {
                processor.PostProcessOrder(order);

                order.PostProcessed = true;

                using (var data = new DataConnection())
                {
                    data.Update(order);
                }

                WriteLog(order, "Order postprocessed");
            }
            catch (Exception ex)
            {
                WriteLog(order, "Unhandled error when postprocessing order", ex);
            }
        }
    }
}
