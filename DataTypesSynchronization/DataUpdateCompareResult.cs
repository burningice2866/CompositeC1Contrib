using System.Collections.Generic;

namespace CompositeC1Contrib.DataTypesSynchronization
{
    public class DataUpdateCompareResult<T>
    {
        public IEnumerable<T> AddedObjects { get; private set; }
        public IEnumerable<T> ModifiedObjects { get; private set; }
        public IEnumerable<T> DeletedObjects { get; private set; }

        public DataUpdateCompareResult(IEnumerable<T> addedObjects, IEnumerable<T> modifiedObjects, IEnumerable<T> deletedObjects)
        {
            AddedObjects = addedObjects;
            ModifiedObjects = modifiedObjects;
            DeletedObjects = deletedObjects;
        }
    }
}
