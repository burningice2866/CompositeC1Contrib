using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

using Composite;
using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email
{
    public static class MailsFacade
    {
        private const string Pattern = @"(?i)\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b";
        private static readonly Regex Regex = new Regex(Pattern, RegexOptions.Compiled);

        public static bool ValidateMailAddress(string email)
        {
            return !Regex.IsMatch(email);
        }

        public static string[] GetMailQueueNames()
        {
            using (var data = new DataConnection())
            {
                return data.Get<IMailQueue>().Select(q => q.Name).ToArray();
            }
        }

        public static IMailQueue GetDefaultMailQueue()
        {
            using (var data = new DataConnection())
            {
                var queue = data.Get<IMailQueue>().FirstOrDefault();
                if (queue == null)
                {
                    throw new InvalidOperationException("There are no queues configured, unable to process mails");
                }

                return queue;
            }
        }

        public static IQueuedMailMessage BuildMessageAndEnqueue(object mailModel)
        {
            var message = MailModelsFacade.BuildEmailMessage(mailModel);

            return EnqueueMessage(message);
        }

        public static IQueuedMailMessage EnqueueMessage(MailMessage mailMessage)
        {
            var defaultQuueue = GetDefaultMailQueue();

            return EnqueueMessage(defaultQuueue, mailMessage);
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

                return EnqueueMessage(queue, mailMessage);
            }
        }

        public static IQueuedMailMessage EnqueueMessage(IMailQueue queue, MailMessage mailMessage)
        {
            Verify.ArgumentNotNull(queue, "queue");
            Verify.ArgumentNotNull(mailMessage, "mailMessage");

            using (var data = new DataConnection())
            {
                if (mailMessage.From == null)
                {
                    mailMessage.From = new MailAddress(queue.From);
                }

                var message = data.CreateNew<IQueuedMailMessage>();

                message.Id = Guid.NewGuid();
                message.TimeStamp = DateTime.UtcNow;
                message.QueueId = queue.Id;
                message.Subject = mailMessage.Subject;
                message.SerializedMessage = MailMessageSerializeFacade.SerializeAsBase64(mailMessage);

                data.Add(message);

                return message;
            }
        }
    }
}
