using System.Configuration;

namespace CompositeC1Contrib.Teasers.Configuration
{
    public class TeasersTemplatePositionCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public TeasersTemplatePositionElement this[int index]
        {
            get { return (TeasersTemplatePositionElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null) BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(TeasersTemplatePositionElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(TeasersTemplatePositionElement element)
        {
            BaseRemove(element);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TeasersTemplatePositionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var e = (TeasersTemplatePositionElement)element;

            return e.Name;
        }
    }
}