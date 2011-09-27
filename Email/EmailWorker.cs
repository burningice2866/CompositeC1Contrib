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
        private volatile bool _running = false;

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
            try
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
                                if (!_running)
                                {
                                    continue;
                                }

                                var messages = data.Get<IEmailMessage>().Where(m => m.QueueId == queue.Id);

                                if (messages.Any())
                                {
                                    var smtpClient = getClient(queue);
                                    if (smtpClient == null)
                                    {
                                        queue.Paused = true;
                                        data.Update(queue);

                                        continue;
                                    }

                                    using (smtpClient)
                                    {
                                        foreach (var message in messages)
                                        {
                                            if (!_running)
                                            {
                                                continue;
                                            }

                                            var mailMessage = EmailFacade.GetMessage(message);

                                            try
                                            {
                                                smtpClient.Send(mailMessage);

                                                Log.LogInformation("Mail message", "Sent mail message " + mailMessage.Subject + " from queue " + queue.Name);

                                                data.Delete(message);
                                            }
                                            catch (Exception exc)
                                            {
                                                Log.LogCritical("Error in sending message", exc);
                                            }                                            
                                        }
                                    }
                                }
                            }
                        }

                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception exc)
            {
                Log.LogCritical("Unhandled error in Email Worker", exc);
            }
        }

        private SmtpClient getClient(IEmailQueue queue)
        {
            try
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
            catch (Exception exc)
            {
                Log.LogCritical("Invalid smtp settings", exc);

                return null;
            }            
        }
    }
}
