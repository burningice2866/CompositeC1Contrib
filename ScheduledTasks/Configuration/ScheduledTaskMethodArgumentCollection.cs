using System.Configuration;

namespace CompositeC1Contrib.ScheduledTasks.Configuration
{
    public class ScheduledTaskMethodArgumentCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public ScheduledTaskMethodArgumentElement this[int index]
        {
            get { return (ScheduledTaskMethodArgumentElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null) BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(ScheduledTaskMethodArgumentElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(ScheduledTaskMethodArgumentElement element)
        {
            BaseRemove(element);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ScheduledTaskMethodArgumentElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var e = (ScheduledTaskMethodArgumentElement)element;

            return e.Name;
        }
    }
}