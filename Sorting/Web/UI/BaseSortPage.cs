using System;
using System.Linq;
using System.Web.UI;

using Composite.C1Console.Events;
using Composite.C1Console.Security;
using Composite.Data;

namespace CompositeC1Contrib.Sorting.Web.UI
{
    public class BaseSortPage : Page
    {
        protected static string hashId(IData data)
        {
            return data.DataSourceId.GetKeyValue().GetHashCode().ToString().Replace("-", String.Empty);
        }

        protected static void updateParents(string seralizedEntityToken, string consoleId)
        {
            var entityToken = EntityTokenSerializer.Deserialize(seralizedEntityToken);
            var graph = new RelationshipGraph(entityToken, RelationshipGraphSearchOption.Both);

            if (graph.Levels.Count<RelationshipGraphLevel>() > 1)
            {
                var level = graph.Levels.ElementAt<RelationshipGraphLevel>(1);
                foreach (var token in level.AllEntities)
                {
                    var consoleMessageQueueItem = new RefreshTreeMessageQueueItem
                    {
                        EntityToken = token
                    };

                    ConsoleMessageQueueFacade.Enqueue(consoleMessageQueueItem, consoleId);
                }
            }
        }
    }
}
