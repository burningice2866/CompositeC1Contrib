using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI.WebControls;

using Composite.Data;

using CompositeC1Contrib.Sorting.Web.UI;
using CompositeC1Contrib.Teasers.Data;
using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.Web.UI
{
    public class SortPageTeasersPage : BaseSortPage 
    {
        protected Repeater rptFields;

        [WebMethod]
        public static void UpdateOrder(string pageId, string position, string consoleId, string entityToken, string serializedOrder)
        {
            var page = PageManager.GetPageById(new Guid(pageId));
            var pageTeasers = TeaserFacade.GetPageTeasers(page, position, false).ToList();

            UpdateOrder(pageTeasers, serializedOrder);

            var serializedEntityToken = HttpUtility.UrlDecode(entityToken);
            if (!String.IsNullOrEmpty(serializedEntityToken))
            {
                updateParents(serializedEntityToken, consoleId);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            var pageId = new Guid(Request.QueryString["pageId"]);
            var page = PageManager.GetPageById(pageId);
            var position = Request.QueryString["position"];
            var pageTeasers = TeaserFacade.GetPageTeasers(page, position, false);

            rptFields.DataSource = pageTeasers;
            rptFields.DataBind();

            base.OnLoad(e);
        }

        private static void UpdateOrder(IList<IPageTeaser> pageTeasers, string serializedOrder)
        {
            var newOrder = new Dictionary<string, int>();

            serializedOrder = serializedOrder.Replace("instance[]=", ",").Replace("&", "");
            var split = serializedOrder.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < split.Length; i++)
            {
                newOrder.Add(split[i], i);
            }

            foreach (var dataScope in new[] { DataScopeIdentifier.Administrated, DataScopeIdentifier.Public })
            {
                using (new DataScope(dataScope))
                {
                    foreach (var instance in pageTeasers)
                    {
                        string number = hashId(instance);

                        if (newOrder.ContainsKey(number) && newOrder[number] != instance.LocalOrdering)
                        {
                            instance.LocalOrdering = newOrder[number];

                            DataFacade.Update(instance);
                        }
                    }
                }
            }
        }
    }
}
