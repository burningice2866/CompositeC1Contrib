using System;

namespace CompositeC1Contrib.Email.Web.UI
{
    public class MailLogItem
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public DateTime TimeStamp { get; set; }
        public string TimeStampString { get; set; }
    }
}
