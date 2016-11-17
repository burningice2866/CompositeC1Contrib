using System.Collections.Generic;

using Composite.C1Console.Elements;
using Composite.C1Console.Security;

namespace CompositeC1Contrib.Composition
{
    public interface IElementActionProviderFor : IProviderFor
    {
        IEnumerable<ElementAction> Provide(EntityToken entityToken);
    }
}
