using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Configuration;

using Composite.C1Console.Events;
using Composite.Core.Threading;
using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public class ECommerceWorker
    {
        private static readonly ECommerceWorker Instance = new ECommerceWorker();
        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        private volatile bool _running;
        private volatile bool _processOrdersNow;
        private readonly Thread _thread;

        private ECommerceWorker()
        {
            GlobalEventSystemFacade.SubscribeToPrepareForShutDownEvent(PrepareForShutDown);

            var threadStart = new ThreadStart(Run);

            _thread = new Thread(threadStart);
        }

        public static void Initialize()
        {
            if (Instance._running)
            {
                return;
            }

            if (ECommerce.OrderProcessor == null)
            {
                return;
            }

            Instance._running = true;

            Instance._thread.Start();
        }

        public static void ProcessOrdersNow()
        {
            Instance._processOrdersNow = true;
        }

        private void PrepareForShutDown(PrepareForShutDownEventArgs e)
        {
            _running = false;
            _processOrdersNow = false;

            _thread.Join(TimeSpan.FromSeconds(5));
        }

        private void Run()
        {
            SetCultureFromWebConfig();

            Utils.WriteLog(null, "Worker is starting, orderprocessor is " + ECommerce.OrderProcessor.GetType().FullName);

            var ticker = 60;

            try
            {
                using (ThreadDataManager.EnsureInitialize())
                {
                    while (_running)
                    {
                        try
                        {
                            if (!_processOrdersNow && ticker != 60)
                            {
                                continue;
                            }

                            _processOrdersNow = false;

                            PostProcessPendingOrders();
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

                            Thread.Sleep(OneSecond);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.WriteLog("Unhandled error in ThreadDataManager, worker is stopping", ex);
            }
        }

        private void PostProcessPendingOrders()
        {
            using (var data = new DataConnection())
            {
                var orders = data.Get<IShopOrder>().Where(s => s.PaymentStatus == (int)PaymentStatus.Authorized && !s.PostProcessed).ToList();
                foreach (var order in orders)
                {
                    if (!_running)
                    {
                        return;
                    }

                    Utils.PostProcessOrder(order, ECommerce.OrderProcessor, data);
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
