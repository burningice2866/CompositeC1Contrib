using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web.Hosting;

using Composite;
using Composite.C1Console.Events;
using Composite.Core;
using Composite.Core.Threading;
using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email
{
    public class MailWorker
    {
        private static readonly MailWorker Instance = new MailWorker();

        private volatile bool _running;
        private readonly Thread _thread;

        private MailWorker()
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
                        SendPendingMessages();

                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception exc)
            {
                Log.LogCritical("Unhandled error in Mail Worker", exc);
            }
        }

        private void SendPendingMessages()
        {
            using (var data = new DataConnection())
            {
                var queues = data.Get<IMailQueue>().Where(q => !q.Paused);

                foreach (var queue in queues)
                {
                    if (!_running)
                    {
                        return;
                    }

                    var messages = data.Get<IQueuedMailMessage>().Where(m => m.QueueId == queue.Id);

                    if (messages.Any())
                    {
                        var smtpClient = GetClient(queue);
                        if (smtpClient == null)
                        {
                            queue.Paused = true;
                            data.Update(queue);

                            return;
                        }

                        using (smtpClient)
                        {
                            foreach (var message in messages)
                            {
                                if (!_running)
                                {
                                    return;
                                }

                                var mailMessage = MailsFacade.GetMailMessage(message);

                                try
                                {
                                    smtpClient.Send(mailMessage);

                                    Log.LogVerbose("Mail message", "Sent mail message " + mailMessage.Subject + " from queue " + queue.Name);

                                    LogSentMailMessage(data, mailMessage, message.QueueId);

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
        }

        private static void LogSentMailMessage(DataConnection data, MailMessage mailMessage, Guid queueId)
        {
            var sentMailMessage = data.CreateNew<ISentMailMessage>();

            sentMailMessage.Id = Guid.NewGuid();
            sentMailMessage.QueueId = queueId;
            sentMailMessage.TimeStamp = DateTime.UtcNow;
            sentMailMessage.Subject = mailMessage.Subject;

            data.Add(sentMailMessage);

            MailMessageFileWriter.SaveMailMessageToDisk(sentMailMessage.Id, mailMessage);
        }

        private static SmtpClient GetClient(IMailQueue queue)
        {
            try
            {
                var smtpClient = new SmtpClient()
                {
                    DeliveryMethod = (SmtpDeliveryMethod)Enum.Parse(typeof(SmtpDeliveryMethod), queue.DeliveryMethod),
                    Host = queue.Host,
                    Port = queue.Port,
                    EnableSsl = queue.EnableSsl,
                    TargetName = queue.TargetName
                };

                if (smtpClient.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
                {
                    Verify.StringNotIsNullOrWhiteSpace(queue.PickupDirectoryLocation);

                    var pickupDirectoryLocation = HostingEnvironment.MapPath(queue.PickupDirectoryLocation);
                    if (!Directory.Exists(pickupDirectoryLocation))
                    {
                        Directory.CreateDirectory(pickupDirectoryLocation);
                    }

                    smtpClient.PickupDirectoryLocation = pickupDirectoryLocation;
                }

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
