using System;
using System.Collections.Generic;

namespace CompositeC1Contrib.Composition
{
    public interface IProviderFor
    {
        IEnumerable<Type> ProviderFor { get; }
    }
}
