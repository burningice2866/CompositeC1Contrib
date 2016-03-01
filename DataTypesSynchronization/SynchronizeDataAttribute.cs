using System;

namespace CompositeC1Contrib.DataTypesSynchronization
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class SynchronizeDataAttribute : Attribute
    {
        public SynchronizationFrequency Frequency { get; set; }
        public Type Mapper { get; set; }
        public Type PostProcessor { get; set; }

        public SynchronizeDataAttribute(SynchronizationFrequency frequency)
        {
            Frequency = frequency;
        }
    }
}
