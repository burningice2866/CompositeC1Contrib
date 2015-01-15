using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Security.Data.Types;

namespace CompositeC1Contrib.Security
{
    public class EvaluatedPagePermissions: ISecurityEvaluator<IPage>
    {
        private static readonly ConcurrentDictionary<Guid, EvaluatedPermissions> Cache = new ConcurrentDictionary<Guid, EvaluatedPermissions>();
        private static IDictionary<Guid, IPagePermissions> _permissionsCache;

        static EvaluatedPagePermissions()
        {
            DataEvents<IPagePermissions>.OnAfterAdd += Flush;
            DataEvents<IPagePermissions>.OnAfterUpdate += Flush;
            DataEvents<IPagePermissions>.OnDeleted += Flush;

            BuildPermissionsCache();
        }

        private static void Flush(object sender, DataEventArgs e)
        {
            Cache.Clear();

            BuildPermissionsCache();
        }

        private static void BuildPermissionsCache()
        {
            using (var data = new DataConnection())
            {
                _permissionsCache = data.Get<IPagePermissions>().ToDictionary(p => p.PageId);
            }
        }

        public bool HasAccess(IPage page)
        {
            var e = GetEvaluatedPermissions(page);

            return PermissionsFacade.HasAccess(e);
        }

        public EvaluatedPermissions GetEvaluatedPermissions(IPage page)
        {
            return Cache.GetOrAdd(page.Id, g =>
            {
                IPagePermissions permission;
                _permissionsCache.TryGetValue(page.Id, out permission);

                return EvaluatePermissions(page.Id, permission);
            });
        }

        public static EvaluatedPermissions EvaluatePermissions(Guid pageId, IPagePermissions permissions)
        {
            var allowedRoles = permissions == null ? null : permissions.AllowedRoles;
            var deniedRoles = permissions == null ? null : permissions.DeniedRoles;

            var evaluatedPermissions = new EvaluatedPermissions
            {
                ExplicitAllowedRoles = PermissionsFacade.Split(allowedRoles).ToArray(),
                ExplicitDeniedRoled = PermissionsFacade.Split(deniedRoles).ToArray()
            };

            if (permissions == null || !permissions.DisableInheritance)
            {
                EvaluateInheritedPermissions(pageId, evaluatedPermissions);
            }

            return evaluatedPermissions;
        }

        private static void EvaluateInheritedPermissions(Guid current, EvaluatedPermissions evaluatedPermissions)
        {
            var allowedRoles = new List<string>();
            var deniedRolews = new List<string>();

            while ((current = PageManager.GetParentId(current)) != Guid.Empty)
            {
                IPagePermissions permissions;
                if (!_permissionsCache.TryGetValue(current, out permissions))
                {
                    continue;
                }

                if (permissions.DisableInheritance)
                {
                    break;
                }

                var ar = PermissionsFacade.Split(permissions.AllowedRoles);
                var dr = PermissionsFacade.Split(permissions.DeniedRoles);

                allowedRoles.AddRange(ar);
                deniedRolews.AddRange(dr);
            }

            evaluatedPermissions.InheritedAllowedRules = allowedRoles.ToArray();
            evaluatedPermissions.InheritedDenieddRules = deniedRolews.ToArray();
        }
    }
}
