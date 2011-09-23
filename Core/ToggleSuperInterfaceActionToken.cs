using System;
using System.Collections.Generic;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Core.Types;

namespace CompositeC1Contrib
{
    [ActionExecutor(typeof(ToggleSuperInterfaceActionExecutor))]
    public class ToggleSuperInterfaceActionToken : ActionToken
    {
        private Type _type;
        public Type InterfaceType
        {
            get { return _type; }
        }

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return new[] { PermissionType.Edit, PermissionType.Administrate }; }
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
}
