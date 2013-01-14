using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CompositeC1Contrib.Web
{
    public static class ContentFilterFacade
    {
        private static List<IContentFilter> _contentFilters;

        static ContentFilterFacade()
        {
            _contentFilters = new List<IContentFilter>();

            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms)
            {
                try
                {
                    var types = asm.GetTypes()
                        .Where(t => typeof(IContentFilter).IsAssignableFrom(t) && !t.IsInterface)
                        .Select(t => (IContentFilter)Activator.CreateInstance(t));

                    _contentFilters.AddRange(types);
                }
                catch { }
            }
        }

        public static void FilterContent(XElement doc, string id)
        {
            foreach (var filter in _contentFilters)
            {
                filter.Filter(doc, id);
            }
        }
    }
}
