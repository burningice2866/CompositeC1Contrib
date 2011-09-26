using System;
using System.Net.Mail;

namespace CompositeC1Contrib.Email.Serialization
{
    [Serializable]
    public class SerializeableMailAddress
    {
        String User;
        String Host;
        String Address;
        String DisplayName;

        public SerializeableMailAddress(MailAddress address)
        {
            User = address.User;
            Host = address.Host;
            Address = address.Address;
            DisplayName = address.DisplayName;
        }

        public MailAddress GetMailAddress()
        {
            return new MailAddress(Address, DisplayName);
        }
    }
}
