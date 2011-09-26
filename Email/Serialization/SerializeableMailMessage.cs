using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace CompositeC1Contrib.Email.Serialization
{
    [Serializable]
    public class SerializeableMailMessage
    {
        Boolean IsBodyHtml { get; set; }
        String Body { get; set; }
        SerializeableMailAddress From { get; set; }
        SerializeableMailAddress Sender { get; set; }
        String Subject { get; set; }
        Encoding BodyEncoding { get; set; }
        Encoding SubjectEncoding { get; set; }
        DeliveryNotificationOptions DeliveryNotificationOptions { get; set; }
        SerializeableCollection Headers { get; set; }
        MailPriority Priority { get; set; }

        IList<SerializeableMailAddress> To { get; set; }
        IList<SerializeableMailAddress> CC { get; set; }
        IList<SerializeableMailAddress> Bcc { get; set; }
        IList<SerializeableMailAddress> ReplyToList { get; set; }

        IList<SerializeableAlternateView> AlternateViews { get; set; }

        IList<SerializeableAttachment> Attachments { get; set; }
       
        public SerializeableMailMessage(MailMessage mailMessage)
        {
            To = new List<SerializeableMailAddress>();
            CC = new List<SerializeableMailAddress>();
            Bcc = new List<SerializeableMailAddress>();
            ReplyToList = new List<SerializeableMailAddress>();

            AlternateViews = new List<SerializeableAlternateView>();

            Attachments = new List<SerializeableAttachment>();

            IsBodyHtml = mailMessage.IsBodyHtml;
            Body = mailMessage.Body;
            Subject = mailMessage.Subject;
            From = new SerializeableMailAddress(mailMessage.From);

            foreach (MailAddress ma in mailMessage.To)
            {
                To.Add(new SerializeableMailAddress(ma));
            }

            foreach (MailAddress ma in mailMessage.CC)
            {
                CC.Add(new SerializeableMailAddress(ma));
            }

            foreach (MailAddress ma in mailMessage.Bcc)
            {
                Bcc.Add(new SerializeableMailAddress(ma));
            }

            Attachments = new List<SerializeableAttachment>();
            foreach (Attachment att in mailMessage.Attachments)
            {
                Attachments.Add(new SerializeableAttachment(att));
            }

            BodyEncoding = mailMessage.BodyEncoding;

            DeliveryNotificationOptions = mailMessage.DeliveryNotificationOptions;
            Headers = new SerializeableCollection(mailMessage.Headers);
            Priority = mailMessage.Priority;

            foreach (MailAddress ma in mailMessage.ReplyToList)
            {
                ReplyToList.Add(new SerializeableMailAddress(ma));
            }

            if (mailMessage.Sender != null)
            {
                Sender = new SerializeableMailAddress(mailMessage.Sender);
            }

            SubjectEncoding = mailMessage.SubjectEncoding;

            foreach (AlternateView av in mailMessage.AlternateViews)
            {
                AlternateViews.Add(new SerializeableAlternateView(av));
            }
        }

        public MailMessage GetMailMessage()
        {
            var mailMessage = new MailMessage()
            {
                IsBodyHtml = IsBodyHtml,
                Body = Body,
                Subject = Subject,
                BodyEncoding = BodyEncoding,
                DeliveryNotificationOptions = DeliveryNotificationOptions,
                Priority = Priority,
                SubjectEncoding = SubjectEncoding,
            };
            
            if (From != null)
            {
                mailMessage.From = From.GetMailAddress();
            }

            foreach (var mailAddress in To)
            {
                mailMessage.To.Add(mailAddress.GetMailAddress());
            }

            foreach (var mailAddress in CC)
            {
                mailMessage.CC.Add(mailAddress.GetMailAddress());
            }

            foreach (var mailAddress in Bcc)
            {
                mailMessage.Bcc.Add(mailAddress.GetMailAddress());
            }

            foreach (var attachment in Attachments)
            {
                mailMessage.Attachments.Add(attachment.GetAttachment());
            }
            
            Headers.CopyTo(mailMessage.Headers);

            foreach (var mailAddress in ReplyToList)
            {
                mailMessage.ReplyToList.Add(mailAddress.GetMailAddress());
            }

            if (Sender != null)
            {
                mailMessage.Sender = Sender.GetMailAddress();
            }

            foreach (var alternateView in AlternateViews)
            {
                mailMessage.AlternateViews.Add(alternateView.GetAlternateView());
            }

            return mailMessage;
        }
    }
}
