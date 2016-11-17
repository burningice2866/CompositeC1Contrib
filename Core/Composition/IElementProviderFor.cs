using System.Collections.Generic;

using Composite.C1Console.Elements;
using Composite.C1Console.Security;

namespace CompositeC1Contrib.Composition
{
    public interface IElementProviderFor : IProviderFor
    {
        IEnumerable<Element> Provide(ElementProviderContext context, EntityToken token);
    }
}
