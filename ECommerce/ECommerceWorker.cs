using System;
using System.Linq;
using System.Threading;

using Composite.C1Console.Events;
using Composite.Core;
using Composite.Core.Threading;
using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public class ECommerceWorker
    {
        private static readonly ECommerceWorker Instance = new ECommerceWorker();

        private volatile bool _running;
        private readonly Thread _thread;

        private ECommerceWorker()
        {
            var threadStart = new ThreadStart(Run);

            _thread = new Thread(threadStart);
        }

        public static void Initialize()
        {
            GlobalEventSystemFacade.SubscribeToPrepareForShutDownEvent(PrepareForShutDown);

            Instance._running = true;

            Instance._thread.Start();
        }

        private static void PrepareForShutDown(PrepareForShutDownEventArgs e)
        {
            Instance._running = false;
        }

        private void Run()
        {
            try
            {
                using (ThreadDataManager.EnsureInitialize())
                {
                    while (_running)
                    {
                        try
                        {
                            PostProcessPendingOrders();

                            Thread.Sleep(1000);
                        }
                        catch (Exception ex)
                        {
                            Log.LogWarning("Unhandled error when postprocessing orders, sleep for 1 minute", ex);

                            Thread.Sleep(60 * 1000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogCritical("Unhandled error in ThreadDataManager, worker is stopping", ex);
            }
        }

        private void PostProcessPendingOrders()
        {
            var orderProcessor = ECommerce.OrderProcessor;
            if (orderProcessor == null)
            {
                return;
            }

            using (var data = new DataConnection())
            {
                var orders = data.Get<IShopOrder>().Where(s => s.PaymentStatus == (int)PaymentStatus.Authorized && !s.PostProcessed);
                foreach (var order in orders)
                {
                    if (!_running)
                    {
                        return;
                    }

                    Utils.PostProcessOrder(order, orderProcessor, data);
                }
            }
        }
    }
}
