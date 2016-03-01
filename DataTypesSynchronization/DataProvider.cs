using System;

namespace CompositeC1Contrib.DataTypesSynchronization
{
    public interface IDataProvider
    {
        bool IsProviderFor(Type dataType);
        object GetData(Type dataType);
    }
}
