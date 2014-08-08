using System;

using Composite.C1Console.Security;
using Composite.C1Console.Security.SecurityAncestorProviders;

namespace CompositeC1Contrib.ECommerce.C1Console.ElementProviders.EntityTokens
{
    [SecurityAncestorProvider(typeof(NoAncestorSecurityAncestorProvider))]
    public class ECommerceElementProviderEntityToken : EntityToken
    {
        public override string Id
        {
            get { return "ECommerceElementProviderEntityToken"; }
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
            return new ECommerceElementProviderEntityToken();
        }

        public override string Serialize()
        {
            return String.Empty;
        }
    }
}
