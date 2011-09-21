using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Core.Types;
using Composite.Data;
using Composite.Data.GeneratedTypes;
using Composite.Plugins.Elements.ElementProviders.GeneratedDataTypesElementProvider;

namespace CompositeC1Contrib.Sorting
{
    [ActionExecutor(typeof(SetDefaultActionExecutor))]
    public sealed class SortableActionToken : ActionToken
    {
        public override IEnumerable<PermissionType> PermissionTypes
        {
            get
            {
                yield return PermissionType.Edit;
                yield return PermissionType.Administrate;
            }
        }

        public override bool IgnoreEntityTokenLocking
        {
            get
            {
                return false;
            }
        }

        public override string Serialize()
        {
            return String.Empty;
        }

        public static ActionToken Deserialize(string serialiedWorkflowActionToken)
        {
            return new SortableActionToken();
        }
    }

    public sealed class SetDefaultActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var token = (GeneratedDataTypesElementProviderTypeEntityToken)entityToken;
            var type = TypeManager.GetType(token.SerializedTypeName);
            Guid guid = type.GetImmutableTypeId();

            var descriptor = DataMetaDataFacade.GetDataTypeDescriptor(guid);

            

            var interfaceType = TypeManager.GetType(descriptor.TypeManagerTypeName);

            var newDataTypeDescriptor = descriptor.Clone();

            

            if (newDataTypeDescriptor.SuperInterfaces.Contains(typeof(IGenericSortable)))
            {
                newDataTypeDescriptor.RemoveSuperInterface(typeof(IGenericSortable));
            }
            else
            {
                newDataTypeDescriptor.AddSuperInterface(typeof(IGenericSortable));
            }

            if (newDataTypeDescriptor.DataScopes.Count == 0)
            {
                newDataTypeDescriptor.DataScopes.Add(DataScopeIdentifier.Public);
            }

            if (descriptor.DataScopes.Count == 0)
            {
                descriptor.DataScopes.Add(DataScopeIdentifier.Public);
            }

            GeneratedTypesFacade.UpdateType(descriptor, newDataTypeDescriptor, true);

            interfaceType = TypeManager.GetType(descriptor.TypeManagerTypeName);

            EntityTokenCacheFacade.ClearCache();

            var treeRefresher = new ParentTreeRefresher(flowControllerServicesContainer);
            treeRefresher.PostRefreshMesseges(entityToken);

            return null;
        }
    }
}
