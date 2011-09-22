using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Core.Types;
using Composite.Data;
using Composite.Data.GeneratedTypes;
using Composite.Plugins.Elements.ElementProviders.GeneratedDataTypesElementProvider;

namespace CompositeC1Contrib
{
    [ActionExecutor(typeof(SetDefaultActionExecutor))]
    public class ToggleSuperInterfaceActionToken : ActionToken
    {
        private Type _type;
        public Type InterfaceType
        {
            get { return _type; }
        }

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
            get { return false; }
        }

        public ToggleSuperInterfaceActionToken(Type interfaceType)
        {
            _type = interfaceType;
        }

        public override string Serialize()
        {
            return _type.FullName;
        }

        public static ActionToken Deserialize(string serialiedWorkflowActionToken)
        {
            var interfaceType = TypeManager.GetType(serialiedWorkflowActionToken);

            return new ToggleSuperInterfaceActionToken(interfaceType);
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
            var superInfterface = ((ToggleSuperInterfaceActionToken)actionToken).InterfaceType;

            var newDataTypeDescriptor = descriptor.Clone();

            if (newDataTypeDescriptor.SuperInterfaces.Contains(superInfterface))
            {
                newDataTypeDescriptor.RemoveSuperInterface(superInfterface);
            }
            else
            {
                newDataTypeDescriptor.AddSuperInterface(superInfterface);
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

            EntityTokenCacheFacade.ClearCache();

            var treeRefresher = new ParentTreeRefresher(flowControllerServicesContainer);
            treeRefresher.PostRefreshMesseges(entityToken);

            return null;
        }
    }
}
