using System;
using System.Linq;
using System.Net.Mail;

using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email
{
    public static class MailsFacade
    {
        public static string[] GetMailQueueNames()
        {
            using (var data = new DataConnection())
            {
                return data.Get<IMailQueue>().Select(q => q.Name).ToArray();
            }
        }

        public static IQueuedMailMessage EnqueueMessage(string queueName, MailMessage mailMessage)
        {
            using (var data = new DataConnection())
            {
                var queue = data.Get<IMailQueue>().SingleOrDefault(q => q.Name == queueName);
                if (queue == null)
                {
                    throw new ArgumentException(String.Format("Unknown queue name '{0}'", queueName), "queueName");
                }

                if (mailMessage.From == null)
                {
                    mailMessage.From = new MailAddress(queue.From);
                }

                var message = data.CreateNew<IQueuedMailMessage>();

                message.Id = Guid.NewGuid();
                message.TimeStamp = DateTime.UtcNow;
                message.QueueId = queue.Id;
                message.Subject = mailMessage.Subject;
                message.SerializedMessage = MailMessageFileWriter.SerializeAsBase64(mailMessage);

                data.Add(message);

                return message;
            }
        }
    }
}
