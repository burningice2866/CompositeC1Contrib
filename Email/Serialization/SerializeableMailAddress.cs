using System;
using System.Net.Mail;

namespace CompositeC1Contrib.Email.Serialization
{
    [Serializable]
    public class SerializeableMailAddress
    {
        private string _user;
        private string _host;
        readonly string _address;
        readonly string _displayName;

        public SerializeableMailAddress(MailAddress address)
        {
            _user = address.User;
            _host = address.Host;
            _address = address.Address;
            _displayName = address.DisplayName;
        }

        public MailAddress GetMailAddress()
        {
            return new MailAddress(_address, _displayName);
        }
    }
}
