using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Forms;
using Composite.C1Console.Forms.DataServices;
using Composite.C1Console.Security;
using Composite.C1Console.Users;
using Composite.C1Console.Workflow;
using Composite.Core.Xml;
using Composite.Data;
using Composite.Data.Transactions;
using Composite.Data.Types;
using Composite.Plugins.Elements.ElementProviders.MediaFileProviderElementProvider;

using CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens;
using CompositeC1Contrib.Workflows;

namespace CompositeC1Contrib.Localization.C1Console.Workflows
{
    [AllowPersistingWorkflow(WorkflowPersistingType.Idle)]
    public sealed class EditResourceWorkflow : Basic1StepDocumentWorkflow
    {
        private const string XmlFilePath = "\\InstalledPackages\\CompositeC1Contrib.Localization\\EditResource.xml";

        public EditResourceWorkflow() : base(null) { }

        private IResourceKey ResourceKey
        {
            get
            {
                var dataToken = (DataEntityToken)EntityToken;

                return (IResourceKey)dataToken.Data;
            }
        }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (BindingExist("Key"))
            {
                return;
            }

            Bindings.Add("Key", ResourceKey.Key);
            Bindings.Add("Type", ResourceKey.Type);

            SetupFormXml();
        }

        public override bool Validate()
        {
            var key = GetBinding<string>("Key");

            if (key != ResourceKey.Key)
            {
                using (var data = new DataConnection())
                {
                    var keyExists = data.Get<IResourceKey>().Any(k => Equals(k.ResourceSet, ResourceKey.ResourceSet) && k.Key == key);
                    if (keyExists)
                    {
                        ShowFieldMessage("ResourceKey", "Resource with this key already exists");

                        return false;
                    }
                }
            }

            return base.Validate();
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            var key = GetBinding<string>("Key");
            var type = GetBinding<string>("Type");

            using (var transaction = TransactionsFacade.CreateNewScope())
            {
                using (var data = new DataConnection())
                {
                    if (key != ResourceKey.Key || type != ResourceKey.Type)
                    {
                        ResourceKey.Key = key;
                        ResourceKey.Type = type;

                        data.Update(ResourceKey);

                        var treeRefresher = CreateSpecificTreeRefresher();

                        treeRefresher.PostRefreshMesseges(new LocalizationElementProviderEntityToken(ResourceKey.ResourceSet));
                    }

                    var resourceValues = data.Get<IResourceValue>().Where(v => v.KeyId == ResourceKey.Id).ToDictionary(v => v.Culture);

                    foreach (var culture in DataLocalizationFacade.ActiveLocalizationCultures)
                    {
                        var bindingKey = GetBindingKey(culture);

                        var value = GetBinding<string>(bindingKey);

                        if (resourceValues.TryGetValue(culture.Name, out IResourceValue resourceValue))
                        {
                            resourceValue.Value = value;

                            data.Update(resourceValue);
                        }
                        else
                        {
                            resourceValue = data.CreateNew<IResourceValue>();

                            resourceValue.Id = Guid.NewGuid();
                            resourceValue.KeyId = ResourceKey.Id;
                            resourceValue.Culture = culture.Name;
                            resourceValue.Value = value;

                            data.Add(resourceValue);
                        }
                    }
                }

                transaction.Complete();
            }

            SetSaveStatus(true);
        }

        private void SetupFormXml()
        {
            var markupProvider = new FormDefinitionFileMarkupProvider(XmlFilePath);

            var formDocument = XDocument.Load(markupProvider.GetReader());

            var layoutXElement = formDocument.Root?.Element(Namespaces.BindingForms10 + FormKeyTagNames.Layout);

            var placeHolderXElement = layoutXElement?.Element(Namespaces.BindingFormsStdUiControls10 + "PlaceHolder");
            if (placeHolderXElement == null)
            {
                return;
            }

            var bindingsXElement = formDocument.Root.Element(Namespaces.BindingForms10 + FormKeyTagNames.Bindings);
            if (bindingsXElement == null)
            {
                return;
            }

            var resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), ResourceKey.Type);
            if (resourceType == ResourceType.Xhtml)
            {
                placeHolderXElement.Name = Namespaces.BindingFormsStdUiControls10 + "TabPanels";
            }

            using (var data = new DataConnection())
            {
                var resourceValues = data.Get<IResourceValue>().Where(v => v.KeyId == ResourceKey.Id).ToDictionary(v => v.Culture);
                var userLocales = UserSettings.GetActiveLocaleCultureInfos(UserSettings.Username).ToList();

                var isAdministratior = PermissionsFacade.IsAdministrator(UserSettings.Username);

                foreach (var culture in DataLocalizationFacade.ActiveLocalizationCultures)
                {
                    var bindingKey = GetBindingKey(culture);
                    var bindingValue = String.Empty;

                    if (resourceValues.TryGetValue(culture.Name, out IResourceValue resourceValue))
                    {
                        bindingValue = resourceValue.Value;
                    }

                    Bindings.Add(bindingKey, bindingValue);

                    bindingsXElement.Add(CreateBindingElement(bindingKey));

                    if (isAdministratior || userLocales.Contains(culture))
                    {
                        placeHolderXElement.Add(CreateEditResourceValueElement(culture, bindingKey, resourceType));
                    }
                    else
                    {
                        placeHolderXElement.Add(CreateReadOnlyResourceValueElement(culture, bindingKey));
                    }
                }
            }

            DeliverFormData(ResourceKey.Key, StandardUiContainerTypes.Document, formDocument.ToString(), Bindings, BindingsValidationRules);
        }

        private static XElement CreateBindingElement(string key)
        {
            return new XElement(Namespaces.BindingForms10 + "binding",
                        new XAttribute("name", key),
                        new XAttribute("type", typeof(string).FullName));
        }

        private static XElement CreateEditResourceValueElement(CultureInfo culture, string key, ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Text:
                case ResourceType.LargeText:
                    {
                        var control = type == ResourceType.Text ? "TextBox" : "TextArea";

                        return new XElement(Namespaces.BindingFormsStdUiControls10 + "FieldGroup", new XAttribute("Label", culture.DisplayName),
                            new XElement(Namespaces.BindingFormsStdUiControls10 + control, new XAttribute("Label", "Value"), new XAttribute("SpellCheck", true),
                                new XElement(Namespaces.BindingFormsStdUiControls10 + control + ".Text",
                                    new XElement(Namespaces.BindingForms10 + "bind", new XAttribute("source", key)
                            ))));
                    }

                case ResourceType.Xhtml:
                    {
                        return new XElement(new XElement(Namespaces.BindingFormsStdUiControls10 + "XhtmlEditor", new XAttribute("Label", culture.DisplayName),
                                new XElement(Namespaces.BindingFormsStdUiControls10 + "XhtmlEditor.Xhtml",
                                    new XElement(Namespaces.BindingForms10 + "bind", new XAttribute("source", key)
                            ))));
                    }

                case ResourceType.Page:
                    {
                        return new XElement(Namespaces.BindingFormsStdUiControls10 + "FieldGroup", new XAttribute("Label", culture.DisplayName),
                            new XElement(Namespaces.BindingFormsStdUiControls10 + "DataReferenceTreeSelector", new XAttribute("Label", "Value"), new XAttribute("NullValueAllowed", false),
                                new XAttribute("Handle", "Composite.Management.PageIdSelectorDialog"),
                                new XAttribute("DataType", typeof(IPage).AssemblyQualifiedName),
                                new XElement(Namespaces.BindingFormsStdUiControls10 + "DataReferenceTreeSelector.Selected",
                                    new XElement(Namespaces.BindingForms10 + "bind", new XAttribute("source", key)
                            ))));
                    }

                case ResourceType.Media:
                    {
                        return new XElement(Namespaces.BindingFormsStdUiControls10 + "FieldGroup", new XAttribute("Label", culture.DisplayName),
                            new XElement(Namespaces.BindingFormsStdUiControls10 + "DataReferenceTreeSelector", new XAttribute("Label", "Value"), new XAttribute("NullValueAllowed", false),
                                new XAttribute("Handle", "Composite.Management.EmbeddableMediaSelectorDialog"),
                                new XAttribute("SearchToken", typeof(MediaFileSearchToken).AssemblyQualifiedName + "|MimeTypes=null, Extensions=null, Folder=null, HideSubfolders='False', Keyword=null"),
                                new XAttribute("DataType", typeof(IMediaFile).AssemblyQualifiedName),
                                new XElement(Namespaces.BindingFormsStdUiControls10 + "DataReferenceTreeSelector.Selected",
                                    new XElement(Namespaces.BindingForms10 + "bind", new XAttribute("source", key)
                            ))));
                    }
            }

            throw new ArgumentOutOfRangeException(nameof(type), $"Specified Resource Type is not supported '{type}'");
        }

        private static XElement CreateReadOnlyResourceValueElement(CultureInfo culture, string key)
        {
            return new XElement(Namespaces.BindingFormsStdUiControls10 + "FieldGroup",
                        new XAttribute("Label", culture.DisplayName),
                        new XElement(Namespaces.BindingFormsStdUiControls10 + "LongText",
                            new XAttribute("Label", "Value"),
                            new XElement(Namespaces.BindingFormsStdUiControls10 + "LongText.Text",
                                new XElement(Namespaces.BindingForms10 + "read",
                                    new XAttribute("source", key)
                        ))));
        }

        private static string GetBindingKey(CultureInfo culture)
        {
            return "Value-" + culture.Name;
        }
    }
}
