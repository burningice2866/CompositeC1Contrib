using System;

using Composite.C1Console.Security;
using Composite.C1Console.Security.SecurityAncestorProviders;

namespace CompositeC1Contrib.Email.ElementProviders.Tokens
{
    [SecurityAncestorProvider(typeof(NoAncestorSecurityAncestorProvider))]
    public class EmailElementProviderEntityToken : EntityToken
    {
        public override string Id
        {
            get { return "EmailElementProviderEntityToken"; }
        }

        public override string Serialize()
        {
            return String.Empty;
        }

        public override string Source
        {
            get { return String.Empty; }
        }

        public override string Type
        {
            get { return String.Empty; }
        }

        public static EntityToken Deserialize(string serializedData)
        {
            return new EmailElementProviderEntityToken();
        }
    }
}
