using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementProvider;
using Composite.C1Console.Security;
using Composite.C1Console.Workflow;
using Composite.Core.ResourceSystem;
using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;
using CompositeC1Contrib.Email.ElementProviders.Actions;
using CompositeC1Contrib.Email.ElementProviders.Tokens;
using CompositeC1Contrib.Email.Workflows;
using Composite.Plugins.Elements.ElementProviders.VirtualElementProvider;

namespace CompositeC1Contrib.Email.ElementProviders
{
    public class EmailElementProvider : IHooklessElementProvider, IAuxiliarySecurityAncestorProvider
    {
        private static readonly ActionGroup _actionGroup = new ActionGroup(ActionGroupPriority.PrimaryHigh);
        private static readonly ActionLocation _actionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = true, ActionGroup = _actionGroup };

        private ElementProviderContext _context;
        public ElementProviderContext Context
        {
            set { _context = value; }
        }

        public EmailElementProvider()
        {
            AuxiliarySecurityAncestorFacade.AddAuxiliaryAncestorProvider<DataEntityToken>(this);
        }

        public IEnumerable<Element> GetChildren(EntityToken entityToken, SearchToken searchToken)
        {
            var dataToken = entityToken as DataEntityToken;
            if (dataToken != null)
            {
                var queue = dataToken.Data as IEmailQueue;
                if (queue != null)
                {
                    var messages = getMessages(queue);

                    foreach (var message in messages)
                    {
                        dataToken = message.GetDataEntityToken();
                        var elementHandle = _context.CreateElementHandle(dataToken);

                        var messageElement = new Element(elementHandle)
                        {
                            VisualData = new ElementVisualizedData
                            {
                                Label = message.Subject,
                                ToolTip = message.Subject,
                                HasChildren = false,
                                Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                                OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                            }
                        };

                        var deleteActionToken = new WorkflowActionToken(WorkflowFacade.GetWorkflowType("Composite.Plugins.Elements.ElementProviders.GeneratedDataTypesElementProvider.DeleteDataWorkflow"));
                        messageElement.AddAction(new ElementAction(new ActionHandle(deleteActionToken))
                        {
                            VisualData = new ActionVisualizedData
                            {
                                Label = "Delete message",
                                ToolTip = "Delete message",
                                Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                                ActionLocation = _actionLocation
                            }
                        });

                        yield return messageElement;
                    }
                }
            }
            else
            {
                var queues = getQueues();

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
                            HasChildren = getMessages(queue).Any(),
                            Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                            OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                        }
                    };

                    var editActionToken = new WorkflowActionToken(typeof(EditEmailQueueWorkflow), new PermissionType[] { PermissionType.Administrate });
                    queueElement.AddAction(new ElementAction(new ActionHandle(editActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Edit queue",
                            ToolTip = "Edit queue",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                            ActionLocation = _actionLocation
                        }
                    });

                    var deleteActionToken = new ConfirmWorkflowActionToken("Are you sure?", typeof(DeleteEmailQueueActionToken));
                    queueElement.AddAction(new ElementAction(new ActionHandle(deleteActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Delete queue",
                            ToolTip = "Delete queue",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                            ActionLocation = _actionLocation
                        }
                    });

                    var toggleStateActionToken = new ToggleEmailQueueStateActionToken(queue.Id);
                    var toggleLabel = queue.Paused ? "Resume" : "Pause";
                    var toggleIcon = queue.Paused ? "accept" : "generated-type-data-delete";

                    queueElement.AddAction(new ElementAction(new ActionHandle(toggleStateActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = toggleLabel,
                            ToolTip = toggleLabel,
                            Icon = new ResourceHandle("Composite.Icons", toggleIcon),
                            ActionLocation = _actionLocation
                        }
                    });

                    yield return queueElement;
                }
            }
        }

        public IEnumerable<Element> GetRoots(SearchToken searchToken)
        {
            var elementHandle = _context.CreateElementHandle(new EmailElementProviderEntityToken());
            var rootElement = new Element(elementHandle)
            {
                VisualData = new ElementVisualizedData
                {
                    Label = "Email",
                    ToolTip = "Email",
                    HasChildren = getQueues().Any(),
                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                    OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                }
            };

            var actionToken = new WorkflowActionToken(typeof(CreateEmailQueueWorkflow));
            rootElement.AddAction(new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Add queue",
                    ToolTip = "Add queue",
                    Icon = new ResourceHandle("Composite.Icons", "generated-type-data-add"),
                    ActionLocation = _actionLocation
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
                if (dataToken != null && dataToken.InterfaceType == typeof(IEmailQueue))
                {
                    dictionary.Add(token, new[] { new EmailElementProviderEntityToken() });
                }
            }

            return dictionary;
        }

        private IEnumerable<IEmailQueue> getQueues()
        {
            using (var data = new DataConnection(PublicationScope.Unpublished))
            {
                return data.Get<IEmailQueue>();
            }
        }

        private IEnumerable<IEmailMessage> getMessages(IEmailQueue queue)
        {
            using (var data = new DataConnection(PublicationScope.Unpublished))
            {
                return data.Get<IEmailMessage>().Where(m => m.QueueId == queue.Id);
            }
        }        
    }
}
