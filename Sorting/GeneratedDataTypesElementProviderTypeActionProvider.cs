using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Composite.C1Console.Elements;
using Composite.C1Console.Security;
using Composite.Core.Types;
using Composite.Data;
using Composite.Plugins.Elements.ElementProviders.GeneratedDataTypesElementProvider;

using CompositeC1Contrib.Composition;

namespace CompositeC1Contrib.Sorting
{
    [Export(typeof(IElementActionProviderFor))]
    public class GeneratedDataTypesElementProviderTypeActionProvider : IElementActionProviderFor
    {
        private static readonly Type SortableType = typeof(IGenericSortable);

        public IEnumerable<Type> ProviderFor
        {
            get { return new[] { typeof(GeneratedDataTypesElementProviderTypeEntityToken) }; }
        }

        public IEnumerable<ElementAction> Provide(EntityToken entityToken)
        {
            var generatedDataTypetoken = (GeneratedDataTypesElementProviderTypeEntityToken)entityToken;
            if (generatedDataTypetoken.Source == "GeneratedDataTypesElementProvider")
            {
                var type = TypeManager.GetType(generatedDataTypetoken.SerializedTypeName);

                if (!typeof(IPageMetaData).IsAssignableFrom(type))
                {
                    string message;
                    string icon;

                    if (typeof(IGenericSortable).IsAssignableFrom(type))
                    {
                        message = "Disable sorting";
                        icon = "delete";
                    }
                    else
                    {
                        message = "Enable sorting";
                        icon = "accept";
                    }

                    var actionToken = new ToggleSuperInterfaceActionToken(SortableType);

                    yield return Actions.CreateInterfaceToggleAction(actionToken, icon, message);
                }
            }
        }
    }
}
