using System;

using Composite.C1Console.Security;
using Composite.C1Console.Security.SecurityAncestorProviders;

namespace CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens
{
    [SecurityAncestorProvider(typeof(NoAncestorSecurityAncestorProvider))]
    public class LocalizationElementProviderEntityToken : EntityToken
    {
        public override string Id => "LocalizationElementProviderEntityToken";

        public override string Source => String.Empty;

        public override string Type => String.Empty;

        public static EntityToken Deserialize(string serializedData)
        {
            return new LocalizationElementProviderEntityToken();
        }

        public override string Serialize()
        {
            return String.Empty;
        }
    }
}
