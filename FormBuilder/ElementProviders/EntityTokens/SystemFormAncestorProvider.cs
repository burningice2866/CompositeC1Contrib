using System.Collections.Generic;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.FormBuilder.ElementProviders.Tokens
{
    public class SystemFormAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            return new [] { new SourceFolderEntityToken("system") };
        }
    }
}
