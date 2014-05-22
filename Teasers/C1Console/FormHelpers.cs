using System;
using System.Collections.Generic;
using System.Linq;

using Composite.Data;

using CompositeC1Contrib.Teasers.Configuration;
using CompositeC1Contrib.Teasers.Data;
using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.C1Console
{
    public static class FormHelpers
    {
        public static Dictionary<string, string> GetDesigns()
        {
            var designs = TeasersSection.GetSection().Designs;
            
            return designs.Count == 0 ? null : designs.Cast<TeasersDesignElement>().ToDictionary(el => el.Name, el => el.Label);
        }

        public static Dictionary<Guid, string> GetSharedTeaserGroups()
        {
            using (var data = new DataConnection())
            {
                return data.Get<ISharedTeaserGroup>().ToDictionary(g => g.Id, g => g.Title);
            }
        }

        public static IEnumerable<KeyValuePair<string, string>> GetSharedTeasers()
        {
            var types = TeaserFacade.GetSharedTeaserTypes();

            return types.SelectMany(type => DataFacade.GetData(type)
                .Cast<ISharedTeaser>()
                ).ToDictionary(t => t.DataSourceId.Serialize(), GetSharedTeaserName).OrderBy(t => t.Value);
        }

        public static Dictionary<string, string> GetPositions(Guid templateId)
        {
            var positions = TeaserElementAttachingProvider.TemplateTeaserPositions[templateId];

            return positions.ToDictionary(p => p.Item1, p => p.Item2);
        }

        private static string GetSharedTeaserName(ISharedTeaser teaser)
        {
            using (var data = new DataConnection())
            {
                var group = data.Get<ISharedTeaserGroup>().SingleOrDefault(g => g.Id == teaser.TeaserGroup);
                
                return group == null ? teaser.Name : String.Format("{0} / {1}", group.Title, teaser.Name);
            }
        }
    }
}
