using System.Configuration;

namespace CompositeC1Contrib.ScheduledTasks.Configuration
{
    public class ScheduledTaskCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public ScheduledTaskElement this[int index]
        {
            get { return (ScheduledTaskElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null) BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(ScheduledTaskElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(ScheduledTaskElement element)
        {
            BaseRemove(element);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ScheduledTaskElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var e = (ScheduledTaskElement)element;

            return e.Name;
        }
    }
}