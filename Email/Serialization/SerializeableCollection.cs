using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CompositeC1Contrib.Email.Serialization
{
    [Serializable]
    public class SerializeableCollection
    {
        IDictionary<string, string> Collection = new Dictionary<string, string>();

        public SerializeableCollection() { }

        public SerializeableCollection(NameValueCollection coll)
        {
            foreach (string key in coll.Keys)
            {
                Collection.Add(key, coll[key]);
            }
        }

        public SerializeableCollection(StringDictionary coll)
        {
            foreach (string key in coll.Keys)
            {
                Collection.Add(key, coll[key]);
            }
        }

        public void CopyTo(NameValueCollection scol)
        {
            foreach (String key in Collection.Keys)
            {
                scol.Add(key, this.Collection[key]);
            }
        }

        public void CopyTo(StringDictionary scol)
        {
            foreach (string key in Collection.Keys)
            {
                if (scol.ContainsKey(key))
                {
                    scol[key] = Collection[key];
                }
                else
                {
                    scol.Add(key, Collection[key]);
                }
            }
        }        
    }
}
