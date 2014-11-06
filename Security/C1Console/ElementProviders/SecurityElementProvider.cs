using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementProvider;
using Composite.C1Console.Security;
using Composite.C1Console.Workflow;
using Composite.Core.ResourceSystem;
using Composite.Data;
using CompositeC1Contrib.Security.C1Console.ElementProviders.Actions;
using CompositeC1Contrib.Security.C1Console.ElementProviders.EntityTokens;
using CompositeC1Contrib.Security.C1Console.Workflows;

namespace CompositeC1Contrib.Security.C1Console.ElementProviders
{
    public class SecurityElementProvider : IHooklessElementProvider, IAuxiliarySecurityAncestorProvider
    {
        private static readonly ActionGroup ActionGroup = new ActionGroup(ActionGroupPriority.PrimaryHigh);
        private static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = true, ActionGroup = ActionGroup };

        private ElementProviderContext _context;
        public ElementProviderContext Context
        {
            set { _context = value; }
        }

        public SecurityElementProvider()
        {
            AuxiliarySecurityAncestorFacade.AddAuxiliaryAncestorProvider<DataEntityToken>(this);
        }

        public IEnumerable<Element> GetChildren(EntityToken entityToken, SearchToken searchToken)
        {
            var folderToken = entityToken as FolderEntityToken;
            if (folderToken != null)
            {
                var folderName = folderToken.Id;
                switch (folderName)
                {
                    case "Users":
                        {
                            int count;
                            var users = Membership.GetAllUsers(0, int.MaxValue, out count).Cast<MembershipUser>().ToList();

                            var approvedCount = users.Count(u => u.IsApproved);

                            var approvedUsersElementHandle = _context.CreateElementHandle(new FolderEntityToken("Approved"));
                            var approvedUsersElement = new Element(approvedUsersElementHandle)
                            {
                                VisualData = new ElementVisualizedData
                                {
                                    Label = String.Format("Approved ({0})", approvedCount),
                                    ToolTip = String.Format("Approved ({0})", approvedCount),
                                    HasChildren = approvedCount > 0,
                                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                                    OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                                }
                            };

                            yield return approvedUsersElement;

                            var notApprovedCount = users.Count(u => !u.IsApproved);

                            var notApprovedUsersElementHandle = _context.CreateElementHandle(new FolderEntityToken("NotApproved"));
                            var notApprovedUsersElement = new Element(notApprovedUsersElementHandle)
                            {
                                VisualData = new ElementVisualizedData
                                {
                                    Label = String.Format("Not approved ({0})", notApprovedCount),
                                    ToolTip = String.Format("Not approved ({0})", notApprovedCount),
                                    HasChildren = notApprovedCount > 0,
                                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                                    OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                                }
                            };

                            yield return notApprovedUsersElement;
                        }

                        break;

                    case "Approved":
                        {
                            int count;
                            var users = Membership.GetAllUsers(0, int.MaxValue, out count).Cast<MembershipUser>();
                            var approved = users.Where(u => u.IsApproved);

                            foreach (var element in GenerateUsersList(approved)) yield return element;
                        }

                        break;

                    case "NotApproved":
                        {
                            int count;
                            var users = Membership.GetAllUsers(0, int.MaxValue, out count).Cast<MembershipUser>();
                            var notApproved = users.Where(u => !u.IsApproved);

                            foreach (var element in GenerateUsersList(notApproved)) yield return element;
                        }

                        break;
                }
            }

            if (entityToken is SecurityElementProviderEntityToken)
            {
                var usersElementHandle = _context.CreateElementHandle(new FolderEntityToken("Users"));
                var usersElement = new Element(usersElementHandle)
                {
                    VisualData = new ElementVisualizedData
                    {
                        Label = "Users",
                        ToolTip = "Users",
                        HasChildren = true,
                        Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                        OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                    }
                };

                var createActionToken = new WorkflowActionToken(typeof(AddUserWorkflow), new[] { PermissionType.Add, PermissionType.Administrate });
                usersElement.AddAction(new ElementAction(new ActionHandle(createActionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Add",
                        ToolTip = "Add",
                        Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                        ActionLocation = ActionLocation
                    }
                });

                yield return usersElement;
            }
        }

        private IEnumerable<Element> GenerateUsersList(IEnumerable<MembershipUser> users)
        {
            foreach (var user in users.OrderBy(u => u.Email))
            {
                var userElementHandle = _context.CreateElementHandle(new UserEntityToken(user));
                var userElement = new Element(userElementHandle)
                {
                    VisualData = new ElementVisualizedData
                    {
                        Label = user.UserName,
                        ToolTip = user.UserName,
                        HasChildren = false,
                        Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                        OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                    }
                };

                var editActionToken = new WorkflowActionToken(typeof(EditUserWorkflow), new[] { PermissionType.Edit, PermissionType.Administrate });
                userElement.AddAction(new ElementAction(new ActionHandle(editActionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Edit",
                        ToolTip = "Edit",
                        Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                        ActionLocation = ActionLocation
                    }
                });

                var resetPasswordActionToken = new ConfirmWorkflowActionToken("Are you want to reset password", typeof(ResetUserPasswordActionToken), new[] { PermissionType.Edit, PermissionType.Administrate });
                userElement.AddAction(new ElementAction(new ActionHandle(resetPasswordActionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Reset password",
                        ToolTip = "Reset password",
                        Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                        ActionLocation = ActionLocation
                    }
                });

                if (!user.IsApproved)
                {
                    var approveActionToken = new ConfirmWorkflowActionToken("Are you want to approve", typeof(ApproveUserActionToken), new[] { PermissionType.Edit, PermissionType.Administrate });
                    userElement.AddAction(new ElementAction(new ActionHandle(approveActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Approve user",
                            ToolTip = "Approve user",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                            ActionLocation = ActionLocation
                        }
                    });
                }

                var deleteActionToken = new ConfirmWorkflowActionToken(String.Format("Are you want to delete {0}", user.Email), typeof(DeleteUserActionToken), new[] { PermissionType.Delete, PermissionType.Administrate });
                userElement.AddAction(new ElementAction(new ActionHandle(deleteActionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Delete",
                        ToolTip = "Delete",
                        Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                        ActionLocation = ActionLocation
                    }
                });

                yield return userElement;
            }
        }

        public IEnumerable<Element> GetRoots(SearchToken searchToken)
        {
            var elementHandle = _context.CreateElementHandle(new SecurityElementProviderEntityToken());
            var rootElement = new Element(elementHandle)
            {
                VisualData = new ElementVisualizedData
                {
                    Label = "Security",
                    ToolTip = "Security",
                    HasChildren = true,
                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                    OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                }
            };

            return new[] { rootElement };
        }

        public Dictionary<EntityToken, IEnumerable<EntityToken>> GetParents(IEnumerable<EntityToken> entityTokens)
        {
            var dictionary = new Dictionary<EntityToken, IEnumerable<EntityToken>>();
            foreach (var token in entityTokens)
            {
                var dataToken = token as DataEntityToken;
                if (dataToken == null)
                {
                    continue;
                }
            }

            return dictionary;
        }
    }
}
