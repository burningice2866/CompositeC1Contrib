using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Composition;

namespace CompositeC1Contrib.Sorting
{
    [Export(typeof(IElementActionProviderFor))]
    public class DataActionProvider : IElementActionProviderFor
    {
        private static readonly Type SortableType = typeof(IGenericSortable);

        public IEnumerable<Type> ProviderFor
        {
            get { return new[] { typeof(DataEntityToken) }; }
        }

        public IEnumerable<ElementAction> Provide(EntityToken entityToken)
        {
            var dataToken = (DataEntityToken)entityToken;
            var type = dataToken.InterfaceType;

            if (typeof(IPage).IsAssignableFrom(type))
            {
                var page = (IPage)dataToken.Data;

                using (var data = new DataConnection(PublicationScope.Unpublished))
                {
                    if (data.Get<IPageStructure>().Count(ps => ps.ParentId == page.Id) > 1)
                    {
                        var url = "SortPages.aspx?pageId=" + page.Id;

                        yield return Actions.CreateSortAction(url, StringResourceSystemFacade.GetString("CompositeC1Contrib.Sorting", "Childpages"));
                    }
                }
            }
        }
    }
}
