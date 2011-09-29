using System;
using System.Collections.Generic;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;

namespace CompositeC1Contrib.Favorites
{
    [ActionExecutor(typeof(RemoveFromFavoritesActionExecutor))]
    public class RemoveFromFavoritesActionToken : ActionToken
    {
        private static IEnumerable<PermissionType> _permissionTypes = new PermissionType[] { PermissionType.Administrate };

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return _permissionTypes; }
        }

        public override string Serialize()
        {
            return String.Empty;
        }

        public static ActionToken Deserialize(string serializedData)
        {
            return new RemoveFromFavoritesActionToken();
        }
    }
}
