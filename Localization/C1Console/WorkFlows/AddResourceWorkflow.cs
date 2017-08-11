using System;
using System.Linq;

using Composite.Data;

using CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens;
using CompositeC1Contrib.Workflows;

namespace CompositeC1Contrib.Localization.C1Console.Workflows
{
    public sealed class AddResourceWorkflow : Basic1StepDialogWorkflow
    {
        public AddResourceWorkflow() : base("\\InstalledPackages\\CompositeC1Contrib.Localization\\AddResource.xml") { }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (BindingExist("ResourceSet"))
            {
                return;
            }

            var resourceSet = String.Empty;
            var ns = String.Empty;

            if (EntityToken is LocalizationEntityToken localizationEntityToken)
            {
                resourceSet = localizationEntityToken.ResourceSet;
            }

            if (EntityToken is NamespaceFolderEntityToken namespaceToken)
            {
                ns = namespaceToken.Namespace + ".";
            }

            Bindings.Add("ResourceSet", resourceSet);
            Bindings.Add("Key", ns);
            Bindings.Add("Type", ResourceType.Text.ToString());
        }

        public override bool Validate()
        {
            var key = GetBinding<string>("Key");

            if (key.IndexOf(".", StringComparison.Ordinal) == -1)
            {
                ShowFieldMessage("ResourceKey", "Resource must contain at least one . seperator");

                return false;
            }

            using (var data = new DataConnection())
            {
                var keyExists = data.Get<IResourceKey>().Any(k => k.Key == key);
                if (keyExists)
                {
                    ShowFieldMessage("ResourceKey", "Resource with this key already exists");

                    return false;
                }
            }

            return base.Validate();
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            var resourceSet = GetBinding<string>("ResourceSet");
            var key = GetBinding<string>("Key");
            var type = GetBinding<string>("Type");

            using (var data = new DataConnection())
            {
                var resourceKey = data.CreateNew<IResourceKey>();

                resourceKey.Id = Guid.NewGuid();
                resourceKey.ResourceSet = String.IsNullOrEmpty(resourceSet) ? null : resourceSet;
                resourceKey.Key = key;
                resourceKey.Type = type;

                resourceKey = data.Add(resourceKey);

                var newResourceEntityToken = resourceKey.GetDataEntityToken();
                var treeRefresher = CreateAddNewTreeRefresher(EntityToken);

                treeRefresher.PostRefreshMesseges(newResourceEntityToken);

                ExecuteWorklow(newResourceEntityToken, typeof(EditResourceWorkflow));
            }
        }
    }
}
