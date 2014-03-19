using System;
using System.Collections.Generic;
using System.Linq;

using Composite.Data;
using CompositeC1Contrib.Teasers.Data;
using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.C1Console
{
    public static class FormHelpers
    {
        public static Dictionary<Guid, string> GetSharedTeaserGroups()
        {
            using (var data = new DataConnection())
            {
                return data.Get<ISharedTeaserGroup>().ToDictionary(g => g.Id, g => g.Title);
            }
        }

        public static Dictionary<string, string> GetSharedTeasers()
        {
            var types = TeaserFacade.GetSharedTeaserTypes();

            return types.SelectMany(type => DataFacade.GetData(type)
                .Cast<ISharedTeaser>()
                ).ToDictionary(t => t.DataSourceId.Serialize(), t => t.Name);
        }

        public static Dictionary<string, string> GetPositions(Guid templateId)
        {
            var positions = TeaserElementAttachingProvider.TemplateTeaserPositions[templateId];

            return positions.ToDictionary(p => p.Item1, p => p.Item2);
        }
    }
}
