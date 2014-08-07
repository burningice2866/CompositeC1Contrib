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
        }

        private void Run()
        {
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

                            PostProcessPendingOrders();

                            _processOrdersNow = false;
                        }
                        catch (Exception ex)
                        {
                            Log.LogWarning("Unhandled error when postprocessing orders", ex);
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
