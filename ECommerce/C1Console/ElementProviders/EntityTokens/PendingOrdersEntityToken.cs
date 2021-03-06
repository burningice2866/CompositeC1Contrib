﻿using System;
using System.Collections.Generic;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.ECommerce.C1Console.ElementProviders.EntityTokens
{
    [SecurityAncestorProvider(typeof(PendingOrdersAncestorProvider))]
    public class PendingOrdersEntityToken : EntityToken
    {
        public override string Id
        {
            get { return "PendingOrdersEntityToken"; }
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
            return new PendingOrdersEntityToken();
        }
    }

    public class PendingOrdersAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            var token = entityToken as PendingOrdersEntityToken;
            if (token == null)
            {
                yield break;
            }

            yield return new ECommerceElementProviderEntityToken();
        }
    }
}
