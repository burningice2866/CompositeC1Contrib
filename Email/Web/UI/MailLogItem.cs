using System;

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
    }
}
