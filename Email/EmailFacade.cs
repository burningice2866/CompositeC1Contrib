using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization.Formatters.Binary;

using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;
using CompositeC1Contrib.Email.Serialization;

namespace CompositeC1Contrib.Email
{
    public static class EmailFacade
    {
        public static string[] GetMailQueueNames()
        {
            using (var data = new DataConnection(PublicationScope.Unpublished))
            {
                return data.Get<IEmailQueue>().Select(q => q.Name).ToArray();
            }
        }

        public static IEmailMessage EnqueueMessage(string queueName, MailMessage mailMessage)
        {
            using (var data = new DataConnection(PublicationScope.Unpublished))
            {
                var queue = data.Get<IEmailQueue>().SingleOrDefault(q => q.Name == queueName);
                if (queue == null)
                {
                    throw new ArgumentException("Unknown queue name", "queueName");
                }

                if (mailMessage.From == null)
                {
                    mailMessage.From = new MailAddress(queue.From);
                }

                var message = data.CreateNew<IEmailMessage>();

                message.Id = Guid.NewGuid();
                message.QueueId = queue.Id;
                message.Subject = mailMessage.Subject;

                using (var ms = new MemoryStream())
                {
                    var serializedMailMessage = new SerializeableMailMessage(mailMessage);

                    new BinaryFormatter().Serialize(ms, serializedMailMessage);
                    message.SerializedMessage = Convert.ToBase64String(ms.ToArray());
                }

                data.Add(message);

                return message;
            }
        }

        public static MailMessage GetMessage(IEmailMessage message)
        {
            byte[] bytes = Convert.FromBase64String(message.SerializedMessage);

            using (var ms = new MemoryStream(bytes))
            {
                var serializedMailMessage = (SerializeableMailMessage)new BinaryFormatter().Deserialize(ms);

                return serializedMailMessage.GetMailMessage();
            }
        }
    }
}
