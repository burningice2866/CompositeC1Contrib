using System.Configuration;

namespace CompositeC1Contrib.Teasers.Configuration
{
	public class TeasersPositionCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

		public TeasersPositionElement this[int index]
        {
			get { return (TeasersPositionElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null) BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

		public void Add(TeasersPositionElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

		public void Remove(TeasersPositionElement element)
        {
            BaseRemove(element);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        protected override ConfigurationElement CreateNewElement()
        {
			return new TeasersPositionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
			var e = (TeasersPositionElement)element;

            return e.Name;
        }
    }
}