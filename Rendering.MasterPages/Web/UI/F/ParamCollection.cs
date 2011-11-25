using System;
using System.Collections;

namespace CompositeC1Contrib.Web.UI.F
{
    public class ParamCollection : IList, ICollection, IEnumerable
    {
        private ArrayList _innerList = new ArrayList();

        public void Add(Param value)
        {
            _innerList.Add(value);
        }

        public void Clear()
        {
            _innerList.Clear();
        }

        public bool Contains(Param value)
        {
            return _innerList.Contains(value);
        }

        public int IndexOf(Param value)
        {
            return _innerList.IndexOf(value);
        }

        public void Insert(int index, Param value)
        {
            _innerList.Insert(index, value);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Remove(Param value)
        {
            _innerList.Remove(value);
        }

        public void RemoveAt(int index)
        {
            _innerList.RemoveAt(index);
        }

        public Param this[int index]
        {
            get { return (Param)_innerList[index]; }
            set { _innerList[index] = value; }
        }

        public void CopyTo(Array array, int index)
        {
            _innerList.CopyTo(array, index);
        }

        public int Count
        {
            get { return _innerList.Count; }
        }

        public bool IsSynchronized
        {
            get { return _innerList.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        public IEnumerator GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        int IList.Add(object value)
        {
            var param = (Param)value;

            return _innerList.Add(param);
        }

        bool IList.Contains(object value)
        {
            return this.Contains((Param)value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((Param)value);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (Param)value);
        }

        void IList.Remove(object value)
        {
            this.Remove((Param)value);
        }

        object IList.this[int index]
        {
            get { return this._innerList[index]; }
            set { this._innerList[index] = (Param)value; }
        }
    }
}
