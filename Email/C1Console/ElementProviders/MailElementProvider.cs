using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementProvider;
using Composite.C1Console.Security;
using Composite.C1Console.Workflow;
using Composite.Core.ResourceSystem;
using Composite.Core.WebClient;
using Composite.Data;

using CompositeC1Contrib.Email.C1Console.ElementProviders.Actions;
using CompositeC1Contrib.Email.C1Console.ElementProviders.EntityTokens;
using CompositeC1Contrib.Email.C1Console.Workflows;
using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email.C1Console.ElementProviders
{
    public class MailElementProvider : IHooklessElementProvider, IAuxiliarySecurityAncestorProvider
    {
        private static readonly ActionGroup ActionGroup = new ActionGroup(ActionGroupPriority.PrimaryHigh);
        private static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = true, ActionGroup = ActionGroup };

        private ElementProviderContext _context;
        public ElementProviderContext Context
        {
            set { _context = value; }
        }

        public MailElementProvider()
        {
            AuxiliarySecurityAncestorFacade.AddAuxiliaryAncestorProvider<DataEntityToken>(this);
        }

        public IEnumerable<Element> GetChildren(EntityToken entityToken, SearchToken searchToken)
        {
            var dataToken = entityToken as DataEntityToken;
            if (dataToken != null)
            {
                var queue = dataToken.Data as IMailQueue;
                if (queue != null)
                {
                    string baseUrl = UrlUtils.ResolveAdminUrl("InstalledPackages/CompositeC1Contrib.Email/log.aspx?queue=" + queue.Id + "&view=");

                    var queuedCount = GetQueuedMessagesCount(queue);
                    var queuedLabel = "Queue";
                    if (queuedCount > 0)
                    {
                        queuedLabel += " (" + queuedCount + ")";
                    }

                    var queuedMailsElementHandle = _context.CreateElementHandle(new QueuedMailsEntityToken(queue));
                    var queuedMailsElement = new Element(queuedMailsElementHandle)
                    {
                        VisualData = new ElementVisualizedData
                        {
                            Label = queuedLabel,
                            ToolTip = "Queued",
                            HasChildren = false,
                            Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                            OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                        }
                    };

                    AddViewLogAction(baseUrl, "queued", queuedMailsElement);

                    yield return queuedMailsElement;

                    var sentCount = GetSentMessagesCount(queue);
                    var sentLabel = "Sent";
                    if (sentCount > 0)
                    {
                        sentLabel += " (" + sentCount + ")";
                    }

                    var sentMailsElementHandle = _context.CreateElementHandle(new SentMailsEntityToken(queue));
                    var sentMailsElement = new Element(sentMailsElementHandle)
                    {
                        VisualData = new ElementVisualizedData
                        {
                            Label = sentLabel,
                            ToolTip = "Sent",
                            HasChildren = false,
                            Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                            OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                        }
                    };

                    AddViewLogAction(baseUrl, "sent", sentMailsElement);

                    yield return sentMailsElement;
                }
            }
            else
            {
                var queues = GetQueues();

                foreach (var queue in queues)
                {
                    var label = queue.Name;

                    if (queue.Paused)
                    {
                        label += " (paused)";
                    }

                    var elementHandle = _context.CreateElementHandle(queue.GetDataEntityToken());
                    var queueElement = new Element(elementHandle)
                    {
                        VisualData = new ElementVisualizedData
                        {
                            Label = label,
                            ToolTip = label,
                            HasChildren = true,
                            Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                            OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                        }
                    };

                    var editActionToken = new WorkflowActionToken(typeof(EditMailQueueWorkflow), new[] { PermissionType.Administrate });
                    queueElement.AddAction(new ElementAction(new ActionHandle(editActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Edit queue",
                            ToolTip = "Edit queue",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                            ActionLocation = ActionLocation
                        }
                    });

                    var deleteActionToken = new ConfirmWorkflowActionToken("Are you sure?", typeof(DeleteMailQueueActionToken));
                    queueElement.AddAction(new ElementAction(new ActionHandle(deleteActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Delete queue",
                            ToolTip = "Delete queue",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                            ActionLocation = ActionLocation
                        }
                    });

                    var toggleStateActionToken = new ToggleMailQueueStateActionToken(queue.Id);
                    var toggleLabel = queue.Paused ? "Resume" : "Pause";
                    var toggleIcon = queue.Paused ? "accept" : "generated-type-data-delete";

                    queueElement.AddAction(new ElementAction(new ActionHandle(toggleStateActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = toggleLabel,
                            ToolTip = toggleLabel,
                            Icon = new ResourceHandle("Composite.Icons", toggleIcon),
                            ActionLocation = ActionLocation
                        }
                    });

                    yield return queueElement;
                }
            }
        }

        private static void AddViewLogAction(string baseUrl, string view, Element element)
        {
            var queuedUrlAction = new UrlActionToken("View log", baseUrl + view, new[] { PermissionType.Administrate });
            element.AddAction(new ElementAction(new ActionHandle(queuedUrlAction))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "View log",
                    ToolTip = "View log",
                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                    ActionLocation = ActionLocation
                }
            });
        }

        public IEnumerable<Element> GetRoots(SearchToken searchToken)
        {
            var elementHandle = _context.CreateElementHandle(new MailElementProviderEntityToken());
            var rootElement = new Element(elementHandle)
            {
                VisualData = new ElementVisualizedData
                {
                    Label = "Email",
                    ToolTip = "Email",
                    HasChildren = GetQueues().Any(),
                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                    OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                }
            };

            var actionToken = new WorkflowActionToken(typeof(CreateMailQueueWorkflow));
            rootElement.AddAction(new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Add queue",
                    ToolTip = "Add queue",
                    Icon = new ResourceHandle("Composite.Icons", "generated-type-data-add"),
                    ActionLocation = ActionLocation
                }
            });

            return new[] { rootElement };
        }

        public Dictionary<EntityToken, IEnumerable<EntityToken>> GetParents(IEnumerable<EntityToken> entityTokens)
        {
            var dictionary = new Dictionary<EntityToken, IEnumerable<EntityToken>>();
            foreach (var token in entityTokens)
            {
                var dataToken = token as DataEntityToken;
                if (dataToken != null && dataToken.InterfaceType == typeof(IMailQueue))
                {
                    dictionary.Add(token, new[] { new MailElementProviderEntityToken() });
                }
            }

            return dictionary;
        }

        private static IEnumerable<IMailQueue> GetQueues()
        {
            using (var data = new DataConnection())
            {
                return data.Get<IMailQueue>();
            }
        }

        private static int GetQueuedMessagesCount(IMailQueue queue)
        {
            using (var data = new DataConnection())
            {
                return data.Get<IQueuedMailMessage>().Count(m => m.QueueId == queue.Id);
            }
        }

        private static int GetSentMessagesCount(IMailQueue queue)
        {
            using (var data = new DataConnection())
            {
                return data.Get<ISentMailMessage>().Count(m => m.QueueId == queue.Id);
            }
        }
    }
}
