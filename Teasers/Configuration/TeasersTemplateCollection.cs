using System.Configuration;

namespace CompositeC1Contrib.Teasers.Configuration
{
    public class TeasersTemplateCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public TeasersTemplateElement this[int index]
        {
            get { return (TeasersTemplateElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null) BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(TeasersTemplateElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(TeasersTemplateElement element)
        {
            BaseRemove(element);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TeasersTemplateElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var e = (TeasersTemplateElement)element;

            return e.Guid;
        }
    }
}