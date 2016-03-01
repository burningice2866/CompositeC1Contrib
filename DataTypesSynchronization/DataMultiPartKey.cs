using System;

using Composite.Data;

namespace CompositeC1Contrib.DataTypesSynchronization
{
    public class DataMultiPartKey : IEquatable<DataMultiPartKey>
    {
        private readonly IData _item;
        private readonly Lazy<int> _hashCode;

        public DataMultiPartKey(IData item)
        {
            _item = item;

            _hashCode = new Lazy<int>(() =>
            {
                var ret = 0;

                foreach (var propertyInfo in _item.DataSourceId.InterfaceType.GetKeyProperties())
                {
                    ret = ret ^ propertyInfo.GetValue(_item, null).GetHashCode();
                }

                return ret;
            });
        }

        public override int GetHashCode()
        {
            return _hashCode.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is DataMultiPartKey && Equals((DataMultiPartKey)obj);
        }

        public bool Equals(DataMultiPartKey other)
        {
            if (_item.DataSourceId.InterfaceType != other._item.DataSourceId.InterfaceType)
            {
                return false;
            }

            foreach (var propertyInfo in _item.DataSourceId.InterfaceType.GetKeyProperties())
            {
                var leftValue = propertyInfo.GetValue(_item, null);
                var rightValue = propertyInfo.GetValue(other._item, null);

                if (!leftValue.Equals(rightValue))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
