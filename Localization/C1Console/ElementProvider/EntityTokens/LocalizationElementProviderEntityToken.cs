using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Security;
using Composite.Data;

namespace CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens
{
    [SecurityAncestorProvider(typeof(LocalizationElementProviderAncestorProvider))]
    public class LocalizationElementProviderEntityToken : LocalizationEntityToken
    {
        public override string Type => nameof(LocalizationElementProviderEntityToken);

        public LocalizationElementProviderEntityToken(string resourceSet) : base(resourceSet) { }

        public static EntityToken Deserialize(string serializedData)
        {
            return new LocalizationElementProviderEntityToken(serializedData);
        }

        public override string Serialize()
        {
            return ResourceSet;
        }
    }

    public class LocalizationElementProviderAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            var providerToken = entityToken as LocalizationElementProviderEntityToken;
            if (providerToken == null)
            {
                return Enumerable.Empty<EntityToken>();
            }

            var resourceSet = providerToken.ResourceSet;
            if (resourceSet.StartsWith("site:"))
            {
                var pageId = Guid.Parse(resourceSet.Remove(0, 5));

                return new[] { PageManager.GetPageById(pageId).GetDataEntityToken() };
            }

            return Enumerable.Empty<EntityToken>();
        }
    }
}
