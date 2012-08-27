using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.FormBuilder.ElementProviders.Tokens
{
    public class SystemFormPropertyAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            var systemFormPropertyEntityToken = entityToken as SystemFormPropertyEntityToken;
            if (systemFormPropertyEntityToken != null)
            {
                return new[] { new SystemFormEntityToken(systemFormPropertyEntityToken.Type) };
            }

            return Enumerable.Empty<EntityToken>();
        }
    }
}
