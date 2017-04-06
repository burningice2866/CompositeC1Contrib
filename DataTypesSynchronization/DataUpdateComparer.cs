using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using Composite.Data;

namespace CompositeC1Contrib.DataTypesSynchronization
{
    public class DataUpdateComparer<T> where T : class, IData
    {
        private readonly IEnumerable<T> _list;
        private readonly IEnumerable<PropertyInfo> _propertiesToIgnore;
        private readonly Logger _logger;
        private readonly CancellationToken _cancellationToken;

        public DataUpdateComparer(IEnumerable<T> list, Logger logger, CancellationToken cancellationToken)
        {
            _list = list;
            _logger = logger;
            _cancellationToken = cancellationToken;

            _propertiesToIgnore = typeof(T).GetProperties().Where(p => p.GetCustomAttribute<SynchronizeIgnoreAttribute>() != null).ToList();
        }

        public DataUpdateCompareResult<T> DataUpdate(bool allowDelete = true)
        {
            using (var data = new DataConnection())
            {
                var existingItems = data.Get<T>().ToDictionary(i => new DataMultiPartKey(i));
                var newItems = new List<T>();
                var updateItems = new List<T>();

                foreach (var itemToCompare in _list)
                {
                    _cancellationToken.ThrowIfCancellationRequested();

                    var key = new DataMultiPartKey(itemToCompare);

                    T existingItem;
                    if (existingItems.TryGetValue(key, out existingItem))
                    {
                        existingItems.Remove(key);

                        if (!PublicInstancePropertiesUpdateCompare(existingItem, itemToCompare))
                        {
                            updateItems.Add(existingItem);
                        }
                    }
                    else
                    {
                        newItems.Add(itemToCompare);
                    }
                }

                if (existingItems.Values.Count > 0 && allowDelete)
                {
                    ClearForeignKeyFields(existingItems.Values, data);

                    _logger.AppendToLog("Deleting {0} items", existingItems.Values.Count);

                    data.Delete<T>(existingItems.Values);
                }

                if (newItems.Count > 0)
                {
                    _logger.AppendToLog("Adding {0} items", newItems.Count);

                    data.Add<T>(newItems);
                }

                if (updateItems.Count > 0)
                {
                    _logger.AppendToLog("Updating {0} items", updateItems.Count);

                    data.Update<T>(updateItems);
                }

                return new DataUpdateCompareResult<T>(newItems, updateItems, existingItems.Values);
            }
        }

        private bool PublicInstancePropertiesUpdateCompare(T existingItem, T newItem)
        {
            if (existingItem == newItem)
            {
                return true;
            }

            var isEqual = true;

            var type = typeof(T);

            foreach (var prop in type.GetProperties())
            {
                if (_propertiesToIgnore.Contains(prop))
                {
                    continue;
                }

                var existingValue = prop.GetValue(existingItem, null);
                var newValue = prop.GetValue(newItem, null);

                if (existingValue == newValue || (existingValue != null && existingValue.Equals(newValue)))
                {
                    continue;
                }

                prop.SetValue(existingItem, newValue, null);

                isEqual = false;
            }

            return isEqual;
        }

        private static void ClearForeignKeyFields(IEnumerable<IData> deletedData, DataConnection data)
        {
            foreach (var deletedInstance in deletedData)
            {
                var deletedInstanceType = deletedInstance.GetType();

                var referees = deletedInstance.GetReferees();
                foreach (var referee in referees)
                {
                    var refereeType = referee.GetType();

                    var interfaces = refereeType.GetInterfaces();
                    foreach (var i in interfaces)
                    {
                        foreach (var prop in i.GetProperties())
                        {
                            var foreignKeyAttribute = prop.GetCustomAttributes(typeof(ForeignKeyAttribute), true).FirstOrDefault() as ForeignKeyAttribute;
                            if (foreignKeyAttribute != null
                                && deletedInstanceType.GetInterfaces().Contains(foreignKeyAttribute.InterfaceType))
                            {
                                var currentValue = prop.GetValue(referee, null);
                                var foreignValue = deletedInstanceType.GetProperty(foreignKeyAttribute.KeyPropertyName).GetValue(deletedInstance, null);

                                if (currentValue.Equals(foreignValue) && !foreignKeyAttribute.AllowCascadeDeletes)
                                {
                                    prop.SetValue(referee, foreignKeyAttribute.NullReferenceValue, null);

                                    data.Update(referee);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
