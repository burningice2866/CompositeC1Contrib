using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementProvider;
using Composite.C1Console.Security;
using Composite.C1Console.Trees.Workflows;
using Composite.C1Console.Workflow;
using Composite.Core.ResourceSystem;
using Composite.Core.Serialization;
using Composite.Data;

using CompositeC1Contrib.FormBuilder.Attributes;
using CompositeC1Contrib.FormBuilder.Data.Types;
using CompositeC1Contrib.FormBuilder.ElementProviders.Actions;
using CompositeC1Contrib.FormBuilder.ElementProviders.Tokens;
using CompositeC1Contrib.FormBuilder.Workflows;

namespace CompositeC1Contrib.FormBuilder.ElementProviders
{
    public class FormBuilderElementProvider : IHooklessElementProvider, IAuxiliarySecurityAncestorProvider
    {
        private static readonly ActionGroup _actionGroup = new ActionGroup(ActionGroupPriority.PrimaryHigh);
        private static readonly ActionLocation _actionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = true, ActionGroup = _actionGroup };

        private ElementProviderContext _context;
        public ElementProviderContext Context
        {
            set { _context = value; }
        }

        public FormBuilderElementProvider()
        {
            AuxiliarySecurityAncestorFacade.AddAuxiliaryAncestorProvider<DataEntityToken>(this);
        }

        public IEnumerable<Element> GetChildren(EntityToken entityToken, SearchToken searchToken)
        {
            var formFolderToken = entityToken as FormFolderEntityToken;
            if (formFolderToken != null)
            {
                if (formFolderToken.Source == "fields")
                {
                    var formId = Guid.Parse(formFolderToken.Id);
                    var elements = getFormFieldElements(formId);

                    foreach (var el in elements)
                    {
                        yield return el;
                    }
                }
                else if (formFolderToken.Source == "instances")
                {

                }
            }

            var dataToken = entityToken as DataEntityToken;
            if (dataToken != null)
            {
                var form = dataToken.Data as IForm;
                if (form != null)
                {
                    var fieldsFoldertHandle = _context.CreateElementHandle(new FormFolderEntityToken(form.Id, "fields"));
                    var fieldsFolderElement = new Element(fieldsFoldertHandle)
                    {
                        VisualData = new ElementVisualizedData
                        {
                            Label = String.Format("Fields ({0})", getFormFieldElements(form.Id).Count()),
                            ToolTip = "Fields",
                            HasChildren = GetFormFields(form.Id).Any(),
                            Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                            OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                        }
                    };

                    if (GetFormFieldTypes().Any())
                    {
                        var createActionToken = new WorkflowActionToken(typeof(CreateFormFieldWorkflow));
                        fieldsFolderElement.AddAction(new ElementAction(new ActionHandle(createActionToken))
                        {
                            VisualData = new ActionVisualizedData
                            {
                                Label = "Add Form field",
                                ToolTip = "Add Form field",
                                Icon = new ResourceHandle("Composite.Icons", "generated-type-data-add"),
                                ActionLocation = _actionLocation
                            }
                        });
                    }

                    yield return fieldsFolderElement;

                    var instancesFoldertHandle = _context.CreateElementHandle(new FormFolderEntityToken(form.Id, "instances"));
                    var instancesFolderElement = new Element(instancesFoldertHandle)
                    {
                        VisualData = new ElementVisualizedData
                        {
                            Label = "Instances",
                            ToolTip = "Instances",
                            HasChildren = false,
                            Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                            OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                        }
                    };

                    yield return instancesFolderElement;
                }
            }

            var sourceFolderEntityToken = entityToken as SourceFolderEntityToken;
            if (sourceFolderEntityToken != null)
            {
                var source = sourceFolderEntityToken.Source;
                var elements = getFormElements(source);

                foreach (var el in elements)
                {
                    yield return el;
                }
            }

            var systemFormEntityToken = entityToken as SystemFormEntityToken;
            if (systemFormEntityToken != null)
            {
                var type = Type.GetType(systemFormEntityToken.Type);
                var props = type.GetProperties();
                var orderedProps = new Dictionary<PropertyInfo, int>();

                foreach (var prop in props)
                {
                    var fieldLabel = prop.GetCustomAttributes(typeof(FieldLabelAttribute), true).FirstOrDefault();
                    if (fieldLabel != null)
                    {
                        var order = prop.GetCustomAttributes(typeof(FieldPositionAttribute), true).FirstOrDefault() as FieldPositionAttribute;

                        orderedProps.Add(prop, order != null ? order.Position : int.MaxValue);
                    }
                }

                foreach (var prop in orderedProps
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key))
                {
                    var elementHandle = _context.CreateElementHandle(new SystemFormPropertyEntityToken(prop));
                    var propertyElement = new Element(elementHandle)
                    {
                        VisualData = new ElementVisualizedData
                        {
                            Label = prop.Name,
                            ToolTip = prop.Name,
                            HasChildren = false,
                            Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                            OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                        }
                    };

                    yield return propertyElement;
                }
            }

            if (entityToken is FormElementProviderEntityToken)
            {
                var sources = getFormsSourceFolders();

                foreach (var source in sources)
                {
                    yield return source;
                }
            }
        }

        public IEnumerable<Element> GetRoots(SearchToken searchToken)
        {
            var elementHandle = _context.CreateElementHandle(new FormElementProviderEntityToken());
            var rootElement = new Element(elementHandle)
            {
                VisualData = new ElementVisualizedData
                {
                    Label = "Forms",
                    ToolTip = "Forms",
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
                if (dataToken != null && dataToken.InterfaceType == typeof(IForm))
                {
                    dictionary.Add(token, new[] { new FormElementProviderEntityToken() });
                }
            }

            return dictionary;
        }

        private IEnumerable<Element> getFormsSourceFolders()
        {
            foreach (var source in new[] { "user", "system" })
            {
                var formSourcesFolder = _context.CreateElementHandle(new SourceFolderEntityToken(source));
                var sourcesFolderElement = new Element(formSourcesFolder)
                {
                    VisualData = new ElementVisualizedData
                    {
                        Label = source,
                        ToolTip = source,
                        HasChildren = getFormElements(source).Any(),
                        Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                        OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                    }
                };

                if (source == "user")
                {
                    var actionToken = new WorkflowActionToken(typeof(CreateFormWorkflow));
                    sourcesFolderElement.AddAction(new ElementAction(new ActionHandle(actionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Add Form",
                            ToolTip = "Add Form",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-add"),
                            ActionLocation = _actionLocation
                        }
                    });
                }

                yield return sourcesFolderElement;
            }
        }

        private IEnumerable<Element> getFormElements(string source)
        {
            if (source == "system")
            {
                var formTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(BaseForm).IsAssignableFrom(t))
                .ToList();

                var types = formTypes;
                foreach (var type in types)
                {
                    var label = type.Name;

                    var elementHandle = _context.CreateElementHandle(new SystemFormEntityToken(type));
                    var formElement = new Element(elementHandle)
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

                    yield return formElement;
                }
            }

            if (source == "user")
            {
                var forms = DataFacade.GetData<IForm>();

                foreach (var form in forms)
                {
                    var label = form.Name;

                    var elementHandle = _context.CreateElementHandle(form.GetDataEntityToken());
                    var formElement = new Element(elementHandle)
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

                    var editActionToken = new WorkflowActionToken(typeof(EditFormWorkflow), new PermissionType[] { PermissionType.Administrate });
                    formElement.AddAction(new ElementAction(new ActionHandle(editActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Edit",
                            ToolTip = "Edit",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                            ActionLocation = _actionLocation
                        }
                    });

                    var deleteActionToken = new WorkflowActionToken(typeof(GenericDeleteDataWorkflow));
                    formElement.AddAction(new ElementAction(new ActionHandle(deleteActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Delete",
                            ToolTip = "Delete",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                            ActionLocation = _actionLocation
                        }
                    });

                    string baseUrl = "/Composite/InstalledPackages/CompositeC1Contrib.FormBuilder/SubmitForm.aspx?formId=" + form.Id;
                    var urlActionToken = new UrlActionToken(label, baseUrl, new[] { PermissionType.Edit, PermissionType.Publish });

                    formElement.AddAction(new ElementAction(new ActionHandle(urlActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Submit form",
                            ToolTip = "Submit form",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-add"),
                            ActionLocation = _actionLocation
                        }
                    });

                    yield return formElement;
                }
            }
        }

        private IEnumerable<Element> getFormFieldElements(Guid formId)
        {
            var fields = GetFormFields(formId);

            foreach (var field in fields)
            {
                var elementHandle = _context.CreateElementHandle(field.GetDataEntityToken());

                var fieldElement = new Element(elementHandle)
                {
                    VisualData = new ElementVisualizedData
                    {
                        Label = field.Label,
                        ToolTip = field.Label,
                        HasChildren = false,
                        Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                        OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                    }
                };

                var payload = new StringBuilder();

                StringConversionServices.SerializeKeyValuePair(payload, "_IconResourceName_", "folder");

                var editActionToken = new WorkflowActionToken(typeof(GenericEditDataWorkflow))
                {
                    Payload = payload.ToString()
                };

                fieldElement.AddAction(new ElementAction(new ActionHandle(editActionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Edit",
                        ToolTip = "Edit",
                        Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                        ActionLocation = _actionLocation
                    }
                });

                var deleteActionToken = new ConfirmWorkflowActionToken("Delete: " + field.Label, typeof(DeleteFormFieldActionToken));
                fieldElement.AddAction(new ElementAction(new ActionHandle(deleteActionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Delete",
                        ToolTip = "Delete",
                        Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                        ActionLocation = _actionLocation
                    }
                });

                yield return fieldElement;
            }
        }

        public static IEnumerable<IFormField> GetFormFields(Guid formId)
        {
            var list = new List<IFormField>();

            using (new DataScope(DataScopeIdentifier.Administrated))
            {
                foreach (var type in GetFormFieldTypes())
                {
                    var instances = DataFacade.GetData(type).Cast<IFormField>().Where(f => f.FormId == formId);
                    foreach (var instance in instances)
                    {
                        list.Add(instance);
                    }
                }
            }

            return list.OrderBy(f => f.LocalOrdering);
        }

        public static IEnumerable<Type> GetFormFieldTypes()
        {
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in ass.GetTypes())
                {
                    if (t.IsInterface && t != typeof(IFormField) && typeof(IFormField).IsAssignableFrom(t))
                    {
                        yield return t;
                    }
                }
            }
        }
    }
}
