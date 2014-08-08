using System;
using System.Collections.Generic;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.ECommerce.C1Console.ElementProviders.EntityTokens
{
    [SecurityAncestorProvider(typeof(AuthorizedOrdersAncestorProvider))]
    public class AuthorizedOrdersEntityToken : EntityToken
    {
        public override string Id
        {
            get { return "AuthorizedOrdersEntityToken"; }
        }

        public override string Source
        {
            get { return String.Empty; }
        }

        public override string Type
        {
            get { return String.Empty; }
        }

        public override string Serialize()
        {
            return DoSerialize();
        }

        public static EntityToken Deserialize(string serializedEntityToken)
        {
            return new AuthorizedOrdersEntityToken();
        }
    }

    public class AuthorizedOrdersAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            var token = entityToken as AuthorizedOrdersEntityToken;
            if (token == null)
            {
                yield break;
            }

            yield return new ECommerceElementProviderEntityToken();
        }
    }
}
