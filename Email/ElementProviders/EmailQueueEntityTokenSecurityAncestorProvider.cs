using System.Collections.Generic;

using Composite.C1Console.Security;

using CompositeC1Contrib.Email.ElementProviders.Tokens;

namespace CompositeC1Contrib.Email.ElementProviders
{
    public class EmailQueueEntityTokenSecurityAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            return new EntityToken[] { new EmailElementProviderEntityToken() };
        }
    }
}
