using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.ElementProviderHelpers.AssociatedDataElementProviderHelper;
using Composite.C1Console.Security;
using Composite.Core.Types;
using Composite.Data;

using CompositeC1Contrib.Composition;

namespace CompositeC1Contrib.Sorting
{
    [Export(typeof(IElementActionProviderFor))]
    public class AssociatedDataElementProviderHelperAtionProvider : IElementActionProviderFor
    {
        private static readonly Type SortableType = typeof(IGenericSortable);        

        public IEnumerable<Type> ProviderFor
        {
            get { return new[] { typeof(AssociatedDataElementProviderHelperEntityToken) }; }
        }

        public IEnumerable<ElementAction> Provide(EntityToken entityToken)
        {
            var associatedToken = (AssociatedDataElementProviderHelperEntityToken)entityToken;

            var type = TypeManager.GetType(associatedToken.Payload);
            if (SortableType.IsAssignableFrom(type))
            {
                var pageId = associatedToken.Id;

                using (new DataScope(DataScopeIdentifier.Administrated))
                {
                    var instances = DataFacade.GetData(type).Cast<IPageFolderData>().Where(f => f.PageId == Guid.Parse(associatedToken.Id));
                    if (instances.Any())
                    {
                        var url = "Sort.aspx?type=" + type.FullName + "&pageId=" + pageId;

                        yield return Actions.CreateSortAction(url, String.Empty);
                    }
                }
            }
        }
    }
}
