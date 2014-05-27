using System;
using System.Collections.Generic;
using System.Linq;

using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email.Web.UI
{
    public class MailLogItem
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public DateTime TimeStamp { get; set; }
        public IMailTemplate Template { get; set; }

        public string FormatTimeStamp()
        {
            var now = DateTime.Now;

            if (TimeStamp.Date == now.Date)
            {
                return TimeStamp.ToString("HH:mm:ss");
            }

            if (TimeStamp.Year == now.Year)
            {
                return TimeStamp.ToString("dd-MM HH:mm:ss");
            }

            return TimeStamp.ToString("dd-MM-yyyy HH:mm:ss");
        }

        public static MailLogItem FromIMailMessage(IMailMessage message)
        {
            using (var data = new DataConnection())
            {
                var templates = data.Get<IMailTemplate>().ToDictionary(t => t.Key);

                return new MailLogItem
                {
                    Id = message.Id,
                    Subject = message.Subject,
                    TimeStamp = message.TimeStamp.ToLocalTime(),
                    Template = GetTemplateForMessage(message, templates)
                };
            }
        }

        private static IMailTemplate GetTemplateForMessage(IMailMessage message, IDictionary<string, IMailTemplate> templates)
        {
            if (String.IsNullOrEmpty(message.MailTemplateKey))
            {
                return null;
            }

            if (!templates.ContainsKey(message.MailTemplateKey))
            {
                return null;
            }

            return templates[message.MailTemplateKey];
        }
    }
}
