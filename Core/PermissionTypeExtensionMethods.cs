using System;
using System.Collections.Generic;
using System.Linq;

using Composite;
using Composite.C1Console.Security;

namespace CompositeC1Contrib
{
    public static class PermissionTypeExtensionMethods
    {
        public static IEnumerable<PermissionType> FromListOfStrings(this IEnumerable<string> permissionTypeNames)
        {
            Verify.ArgumentNotNull(permissionTypeNames, "permissionTypeNames");

            return permissionTypeNames.Select(permissionTypeName => (PermissionType)Enum.Parse(typeof(PermissionType), permissionTypeName));
        }

        public static string SerializePermissionTypes(this IEnumerable<PermissionType> permissionTypes)
        {
            Verify.ArgumentNotNull(permissionTypes, "permissionTypes");

            return String.Join("�", permissionTypes);
        }

        public static IEnumerable<PermissionType> DeserializePermissionTypes(this string serializedPermissionTypes)
        {
            Verify.ArgumentNotNull(serializedPermissionTypes, "serializedPermissionTypes");

            var split = serializedPermissionTypes.Split(new[] { '�' }, StringSplitOptions.RemoveEmptyEntries);

            return split.Select(s => (PermissionType)Enum.Parse(typeof(PermissionType), s));
        }
    }
}
