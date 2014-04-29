using System;
using System.Linq;
using System.Web.UI;

using Composite.C1Console.Events;
using Composite.C1Console.Security;

namespace CompositeC1Contrib.Email.Web.UI
{
    public abstract class BasePage : Page
    {
        protected string View
        {
            get { return Request.QueryString["view"]; }
        }

        protected string EntityToken
        {
            get { return Request.QueryString["EntityToken"]; }
        }

        protected string ConsoleId
        {
            get { return Request.QueryString["consoleId"]; }
        }

        protected string BaseUrl
        {
            get
            {
                var qs = Request.QueryString;

                return String.Format("?queue={0}&view={1}&consoleId={2}&EntityToken={3}", qs["queue"], View, ConsoleId, EntityToken);
            }
        }

        protected void UpdateParents()
        {
            var entityToken = EntityTokenSerializer.Deserialize(EntityToken);
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

                ConsoleMessageQueueFacade.Enqueue(consoleMessageQueueItem, ConsoleId);
            }
        }
    }
}
