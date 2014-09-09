using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementProvider;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Data;

using CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.Actions;
using CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.EntityTokens;

using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;

namespace CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders
{
    public class ScheduledTasksElementProvider : IHooklessElementProvider, IAuxiliarySecurityAncestorProvider
    {
        private static readonly ActionGroup ActionGroup = new ActionGroup(ActionGroupPriority.PrimaryHigh);
        private static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = true, ActionGroup = ActionGroup };

        private ElementProviderContext _context;
        public ElementProviderContext Context
        {
            set { _context = value; }
        }

        public ScheduledTasksElementProvider()
        {
            AuxiliarySecurityAncestorFacade.AddAuxiliaryAncestorProvider<DataEntityToken>(this);
        }

        public IEnumerable<Element> GetChildren(EntityToken entityToken, SearchToken searchToken)
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var connection = JobStorage.Current.GetConnection();
            var statistics = monitoringApi.GetStatistics();

            var folderToken = entityToken as FolderEntityToken;
            if (folderToken != null)
            {
                if (folderToken.TaskType == TaskType.Recurring)
                {
                    foreach (var job in connection.GetRecurringJobs())
                    {
                        var recurringTaskElementHandle = _context.CreateElementHandle(new TaskEntityToken(TaskType.Recurring, job.Id));
                        var recurringTaskElement = new Element(recurringTaskElementHandle)
                        {
                            VisualData = new ElementVisualizedData
                            {
                                Label = job.Id,
                                ToolTip = job.Id,
                                HasChildren = false,
                                Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                                OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                            }
                        };

                        var triggerActionToken = new TriggerRecurringTaskActionToken();
                        recurringTaskElement.AddAction(new ElementAction(new ActionHandle(triggerActionToken))
                        {
                            VisualData = new ActionVisualizedData
                            {
                                Label = "Trigger now",
                                ToolTip = "Trigger now",
                                Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                                ActionLocation = ActionLocation
                            }
                        });

                        var deleteActionToken = new ConfirmWorkflowActionToken("Are you sure?", typeof(DeleteRecurringTaskActionToken));
                        recurringTaskElement.AddAction(new ElementAction(new ActionHandle(deleteActionToken))
                        {
                            VisualData = new ActionVisualizedData
                            {
                                Label = "Delete",
                                ToolTip = "Delete",
                                Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                                ActionLocation = ActionLocation
                            }
                        });

                        yield return recurringTaskElement;
                    }
                }

                if (folderToken.TaskType == TaskType.Scheduled)
                {
                    foreach (var job in monitoringApi.ScheduledJobs(0, int.MaxValue))
                    {
                        var scheduledTaskElementHandle = _context.CreateElementHandle(new TaskEntityToken(TaskType.Scheduled, job.Key));
                        var scheduledTaskElement = new Element(scheduledTaskElementHandle)
                        {
                            VisualData = new ElementVisualizedData
                            {
                                Label = DisplayJob(job.Value.Job),
                                ToolTip = DisplayJob(job.Value.Job),
                                HasChildren = false,
                                Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                                OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                            }
                        };

                        var requeueActionToken = new RequeueScheduledTaskActionToken();
                        scheduledTaskElement.AddAction(new ElementAction(new ActionHandle(requeueActionToken))
                        {
                            VisualData = new ActionVisualizedData
                            {
                                Label = "Enqueue now",
                                ToolTip = "Enqueue now",
                                Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                                ActionLocation = ActionLocation
                            }
                        });

                        var deleteActionToken = new ConfirmWorkflowActionToken("Are you sure?", typeof(DeleteScheduledTaskActionToken));
                        scheduledTaskElement.AddAction(new ElementAction(new ActionHandle(deleteActionToken))
                        {
                            VisualData = new ActionVisualizedData
                            {
                                Label = "Delete",
                                ToolTip = "Delete",
                                Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                                ActionLocation = ActionLocation
                            }
                        });

                        yield return scheduledTaskElement;
                    }
                }
            }

            if (entityToken is ScheduledTasksElementProviderEntityToken)
            {
                var recurringTasksElementHandle = _context.CreateElementHandle(new FolderEntityToken(TaskType.Recurring));
                var recurringTasksElement = new Element(recurringTasksElementHandle)
                {
                    VisualData = new ElementVisualizedData
                    {
                        Label = "Recurring tasks",
                        ToolTip = "Recurring tasks",
                        HasChildren = statistics.Recurring > 0,
                        Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                        OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                    }
                };

                if (statistics.Recurring > 0)
                {
                    recurringTasksElement.VisualData.Label += " (" + statistics.Recurring + " )";
                }

                AddViewAction(TaskType.Recurring, recurringTasksElement);

                yield return recurringTasksElement;

                var scheduledTasksElementHandle = _context.CreateElementHandle(new FolderEntityToken(TaskType.Scheduled));
                var scheduledTasksElement = new Element(scheduledTasksElementHandle)
                {
                    VisualData = new ElementVisualizedData
                    {
                        Label = "Scheduled tasks",
                        ToolTip = "Scheduled tasks",
                        HasChildren = statistics.Scheduled > 0,
                        Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                        OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                    }
                };

                if (statistics.Scheduled > 0)
                {
                    scheduledTasksElement.VisualData.Label += " (" + statistics.Scheduled + " )";
                }

                AddViewAction(TaskType.Scheduled, scheduledTasksElement);

                yield return scheduledTasksElement;
            }
        }

        public IEnumerable<Element> GetRoots(SearchToken searchToken)
        {
            var elementHandle = _context.CreateElementHandle(new ScheduledTasksElementProviderEntityToken());
            var rootElement = new Element(elementHandle)
            {
                VisualData = new ElementVisualizedData
                {
                    Label = "Scheduled tasks",
                    ToolTip = "Scheduled tasks",
                    HasChildren = true,
                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                    OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                }
            };

            return new[] { rootElement };
        }

        private static void AddViewAction(TaskType type, Element element)
        {
            var url = "/hangfire";

            switch (type)
            {
                case TaskType.Scheduled: url += "/scheduled"; break;
                case TaskType.Recurring: url += "/recurring"; break;
            }

            var urlAction = new UrlActionToken("View log", url, new[] { PermissionType.Administrate });
            element.AddAction(new ElementAction(new ActionHandle(urlAction))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "View",
                    ToolTip = "View",
                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                    ActionLocation = ActionLocation
                }
            });
        }

        public static string DisplayJob(Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job");
            }

            var attribute = Attribute.GetCustomAttribute(job.Method, typeof(DisplayNameAttribute), true) as DisplayNameAttribute;
            if ((attribute == null) || (attribute.DisplayName == null))
            {
                return String.Format("{0}.{1}", job.Type.Name, job.Method.Name);
            }
            try
            {
                var args = job.Arguments.ToArray<object>();

                return String.Format(attribute.DisplayName, args);
            }
            catch (FormatException)
            {
                return attribute.DisplayName;
            }
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
