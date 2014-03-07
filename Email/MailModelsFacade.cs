using System;
using System.Linq;
using System.Net.Mail;

using Composite;
using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email
{
    public class MailModelsFacade
    {
        public static MailMessage BuildEmailMessage(object mailModel)
        {
            using (var data = new DataConnection())
            {
                var modelType = mailModel.GetType();

                var template = data.Get<IMailTemplate>()
                    .SingleOrDefault(t => !String.IsNullOrEmpty(t.ModelType) && Type.GetType(t.ModelType) == modelType);

                if (template == null)
                {
                    throw new ArgumentException("There is either zero or more than one template using the supplied model");
                }

                return BuildEmailMessage(template, mailModel);
            }
        }

        public static MailMessage BuildEmailMessage(string key, object mailModel)
        {
            using (var data = new DataConnection())
            {
                var template = data.Get<IMailTemplate>().SingleOrDefault(t => String.Compare(t.Key, key, StringComparison.OrdinalIgnoreCase) == 0);

                Verify.IsNotNull(template, "Unknown template '{0}'", key);

                return BuildEmailMessage(template, mailModel);
            }
        }

        public static MailMessage BuildEmailMessage(IMailTemplate template, object mailModel)
        {
            var msg = new MailMessage()
            {
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
