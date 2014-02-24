using System;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;

using Composite;
using Composite.Data;
using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email
{
    public class MailModelsFacade
    {
        public static MailMessage BuildEmailMessage(string key, object mailModel)
        {
            using (var data = new DataConnection())
            {
                var template = data.Get<IMailTemplate>().SingleOrDefault(t => String.Compare(t.Key, key, StringComparison.OrdinalIgnoreCase) == 0);

                Verify.IsNotNull(template, "Unknown template '{0}'", key);

                var configuration = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                var from = String.IsNullOrEmpty(template.From) ? configuration.From : template.From;

                Verify.IsNotNull(from, "Missing from address");

                var msg = new MailMessage()
                {
                    From = new MailAddress(from),
                    Subject = TemplateRenderer.ResolveText(template.Subject, mailModel),
                    Body = TemplateRenderer.ResolveHtml(template.Body, mailModel),
                    IsBodyHtml = true,
                };

                if (!String.IsNullOrEmpty(template.To))
                {
                    msg.To.Add(new MailAddress(template.To));
                }

                if (!String.IsNullOrEmpty(template.Bcc))
                {
                    msg.Bcc.Add(template.Bcc);
                }

                return msg;
            }
        }
    }
}
