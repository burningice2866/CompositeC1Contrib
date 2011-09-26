using System;

using Composite.C1Console.Security;
using Composite.C1Console.Security.SecurityAncestorProviders;

namespace CompositeC1Contrib.Email.ElementProviders.Tokens
{
    [SecurityAncestorProvider(typeof(EmailQueueEntityTokenSecurityAncestorProvider))]
    public class EmailQueueEntityToken : EntityToken
    {
        private string _id;
        public override string Id
        {
            get { return _id; }
        }

        public override string Source
        {
            get { return String.Empty; }
        }

        public override string Type
        {
            get { return String.Empty; }
        }

        public EmailQueueEntityToken(string id)
        {
            _id = id;
        }

        public override string Serialize()
        {
            return DoSerialize();
        }

        public static EntityToken Deserialize(string serializedData)
        {
            string type, source, id;

            DoDeserialize(serializedData, out type, out source, out id);

            return new EmailQueueEntityToken(id);
        }
    }
}
