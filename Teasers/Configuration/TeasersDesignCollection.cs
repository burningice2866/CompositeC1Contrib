using System.Configuration;

namespace CompositeC1Contrib.Teasers.Configuration
{
    public class TeasersDesignCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public TeasersDesignElement this[int index]
        {
            get { return (TeasersDesignElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null) BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(TeasersDesignElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(TeasersDesignElement element)
        {
            BaseRemove(element);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TeasersDesignElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var e = (TeasersDesignElement)element;

            return e.Name;
        }
    }
}