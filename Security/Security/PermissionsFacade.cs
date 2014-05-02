using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Security;

using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Security.Data.Types;

namespace CompositeC1Contrib.Security.Security
{
    public static class PermissionsFacade
    {
        public static bool HasAccess(SiteMapNode node)
        {
            var page = PageManager.GetPageById(new Guid(node.Key));

            return HasAccess(page);
        }

        public static bool HasAccess(IPage page)
        {
            if (page.Id.ToString() == SiteMap.RootNode.Key)
            {
                return true;
            }

            using (var data = new DataConnection())
            {
                var permission = data.Get<IPagePermissions>().SingleOrDefault(p => p.PageId == page.Id);
                if (permission != null)
                {
                    var allowedRoles = permission.AllowedRoles == null ? new string[0] : permission.AllowedRoles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var deniedRoles = permission.DeniedRoles == null ? new string[0] : permission.DeniedRoles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (deniedRoles.Length > 0 || allowedRoles.Length > 0)
                    {
                        var principal = Thread.CurrentPrincipal;
                        var userRoles = new List<string>();

                        if (principal.Identity.IsAuthenticated)
                        {
                            userRoles.AddRange(Roles.GetRolesForUser());

                            userRoles.Add("AUTHENTICATED");
                        }
                        else
                        {
                            userRoles.Add("ANONYMOUS");
                        }

                        if (deniedRoles.Intersect(userRoles).Any())
                        {
                            return false;
                        }

                        if (!allowedRoles.Intersect(userRoles).Any())
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}