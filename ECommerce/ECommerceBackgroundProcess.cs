using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Configuration;

using Composite.Core.Threading;
using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

using Hangfire.Server;

namespace CompositeC1Contrib.ECommerce
{
    public class ECommerceBackgroundProcess : IBackgroundProcess
    {
        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
        private const int NumberOfRecordsInSinglePass = 10;

        private static volatile bool _processOrdersNow;

        public static void ProcessOrdersNow()
        {
            _processOrdersNow = true;
        }

        public void Execute(BackgroundProcessContext context)
        {
            try
            {
                SetCultureFromWebConfig();

                Utils.WriteLog("Worker is starting, orderprocessor is " + ECommerce.OrderProcessor.GetType().FullName);

                var ticker = 60;

                using (ThreadDataManager.EnsureInitialize())
                {
                    while (!context.IsShutdownRequested)
                    {
                        try
                        {
                            if (!_processOrdersNow && ticker != 60)
                            {
                                continue;
                            }

                            _processOrdersNow = false;

                            PostProcessPendingOrders(context);
                        }
                        catch (Exception ex)
                        {
                            Utils.WriteLog("Unhandled error when postprocessing orders", ex);
                        }
                        finally
                        {
                            if (ticker == 60)
                            {
                                ticker = 0;
                            }

                            ticker = ticker + 1;

                            context.CancellationToken.WaitHandle.WaitOne(OneSecond);
                            context.CancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Utils.WriteLog("Unhandled error in ThreadDataManager, worker is stopping", ex);
            }
        }

        private static void PostProcessPendingOrders(BackgroundProcessContext context)
        {
            using (var data = new DataConnection())
            {
                var orders = data.Get<IShopOrder>()
                    .Where(s => s.PaymentStatus == (int)PaymentStatus.Authorized && !s.PostProcessed)
                    .Take(NumberOfRecordsInSinglePass).ToList();

                foreach (var order in orders)
                {
                    Utils.PostProcessOrder(order, ECommerce.OrderProcessor, data);

                    context.CancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        private static void SetCultureFromWebConfig()
        {
            var globalization = ConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
            if (globalization == null)
            {
                return;
            }

            if (!String.IsNullOrEmpty(globalization.Culture))
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(globalization.Culture);
            }

            if (!String.IsNullOrEmpty(globalization.UICulture))
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(globalization.UICulture);
            }
        }
    }
}
