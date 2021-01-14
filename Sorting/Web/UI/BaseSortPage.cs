using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

using Composite.C1Console.Events;
using Composite.C1Console.Security;
using Composite.Data;

namespace CompositeC1Contrib.Sorting.Web.UI
{
    public class BaseSortPage : Page
    {
        protected new SortMasterPage Master => (SortMasterPage)base.Master;

        protected static string HashId(IData data)
        {
            var keyValue = data.DataSourceId.GetKeyValue().ToString();
            var inputBytes = Encoding.UTF8.GetBytes(keyValue);

            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();

                for (var i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }

        protected static void UpdateParents(string serializedEntityToken, string consoleId)
        {
            var entityToken = EntityTokenSerializer.Deserialize(serializedEntityToken);
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
            for (var i = 0; i < split.Length; i++)
            {
                newOrder.Add(split[i], i);
            }

            return newOrder;
        }
    }
}
