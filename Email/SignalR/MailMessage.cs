using CompositeC1Contrib.Email.Data.Types;
using CompositeC1Contrib.Email.Web.UI;

namespace CompositeC1Contrib.Email.SignalR
{
    public class MailMessage
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string QueueId { get; set; }
        public string TemplateKey { get; set; }
        public string TimeStamp { get; set; }

        public static MailMessage FromIMailMessage(IMailMessage message)
        {
            var logItem = MailLogItem.FromIMailMessage(message);

            return new MailMessage
            {
                Id = message.Id.ToString(),
                QueueId = message.QueueId.ToString(),
                TemplateKey = message.MailTemplateKey ?? "No template",
                Subject = message.Subject,
                TimeStamp = logItem.FormatTimeStamp()
            };
        }
    }
}
