using System;
using System.Reflection;

namespace CompositeC1Contrib.DataTypesSynchronization
{
    public class SynchronizationTypeDefinition
    {
        public Type Type { get; set; }
        public SynchronizationFrequency Frequency { get; set; }
        public MethodInfo Method { get; set; }
        public object Mapper { get; set; }
        public object PostProcessor { get; set; }
    }
}
