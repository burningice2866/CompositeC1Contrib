using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;

using Composite.C1Console.Events;
using Composite.Core;
using Composite.Core.Threading;
using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email
{
    public class EmailWorker
    {
        private static EmailWorker _instance = new EmailWorker();

        private Thread _thread;
        private bool _running = false;

        private EmailWorker()
        {
            var threadStart = new ThreadStart(run);
            _thread = new Thread(threadStart);
        }

        public static void Initialize()
        {
            GlobalEventSystemFacade.SubscribeToPrepareForShutDownEvent(prepareFormShutDownEvent);

            _instance._running = true;
            _instance._thread.Start();
        }

        private static void prepareFormShutDownEvent(PrepareForShutDownEventArgs e)
        {
            _instance._running = false;
        }

        private void run()
        {
            using (ThreadDataManager.EnsureInitialize())
            {
                while (_running)
                {
                    using (var data = new DataConnection())
                    {
                        var queues = data.Get<IEmailQueue>().Where(q => !q.Paused);

                        foreach (var queue in queues)
                        {
                            var messages = data.Get<IEmailMessage>().Where(m => m.QueueId == queue.Id);

                            if (messages.Any())
                            {
                                using (var smtpClient = getClient(queue))
                                {
                                    foreach (var message in messages)
                                    {
                                        var mailMessage = EmailFacade.GetMessage(message);

                                        smtpClient.Send(mailMessage);

                                        Log.LogInformation("Mail message", "Sent mail message " + mailMessage.Subject + " from queue " + queue.Name);

                                        data.Delete(message);
                                    }
                                }
                            }
                        }
                    }

                    Thread.Sleep(1000);
                }
            }
        }

        private SmtpClient getClient(IEmailQueue queue)
        {
            var smtpClient = new SmtpClient()
            {
                DeliveryMethod = (SmtpDeliveryMethod)Enum.Parse(typeof(SmtpDeliveryMethod), queue.DeliveryMethod),
                Host = queue.Host,
                Port = queue.Port,
                EnableSsl = queue.EnableSsl,
                TargetName = queue.TargetName,
                PickupDirectoryLocation = queue.PickupDirectoryLocation
            };

            if (queue.DefaultCredentials)
            {
                smtpClient.Credentials = (NetworkCredential)CredentialCache.DefaultCredentials;
            }
            else if (!String.IsNullOrEmpty(queue.UserName) && !String.IsNullOrEmpty(queue.Password))
            {
                smtpClient.Credentials = new NetworkCredential(queue.UserName, queue.Password);
            }

            return smtpClient;
        }
    }
}
