using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;

using Composite.C1Console.Events;
using Composite.C1Console.Security;
using Composite.Data;

namespace CompositeC1Contrib.Sorting.Web.UI
{
    public class BaseSortPage : Page
    {
        protected new SortMasterPage Master
        {
            get { return (SortMasterPage)base.Master; }
        }

        protected static string HashId(IData data)
        {
            return data.DataSourceId.GetKeyValue().GetHashCode().ToString(CultureInfo.InvariantCulture).Replace("-", String.Empty);
        }

        protected static void UpdateParents(string seralizedEntityToken, string consoleId)
        {
            var entityToken = EntityTokenSerializer.Deserialize(seralizedEntityToken);
            var graph = new RelationshipGraph(entityToken, RelationshipGraphSearchOption.Both);

            if (graph.Levels.Count() <= 1)
            {
                return;
            }

            var level = graph.Levels.ElementAt(1);
            foreach (var token in level.AllEntities)
            {
                var consoleMessageQueueItem = new RefreshTreeMessageQueueItem
                {
                    EntityToken = token
                };

                ConsoleMessageQueueFacade.Enqueue(consoleMessageQueueItem, consoleId);
            }
        }

        protected static IDictionary<string, int> ParseNewOrder(string serializedOrder)
        {
            var newOrder = new Dictionary<string, int>();

            serializedOrder = serializedOrder.Replace("instance[]=", ",").Replace("&", String.Empty);
            var split = serializedOrder.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < split.Length; i++)
            {
                newOrder.Add(split[i], i);
            }

            return newOrder;
        }
    }
}
