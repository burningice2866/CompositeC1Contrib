using System;
using System.Linq;

using Composite.Data;

using CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens;
using CompositeC1Contrib.Workflows;

namespace CompositeC1Contrib.Localization.C1Console.Workflows
{
    public sealed class RenameNamespaceWorkflow : Basic1StepDialogWorkflow
    {
        public RenameNamespaceWorkflow() : base("\\InstalledPackages\\CompositeC1Contrib.Localization\\RenameNamespace.xml") { }

        private NamespaceFolderEntityToken NamespaceFolderToken
        {
            get { return (NamespaceFolderEntityToken)EntityToken; }
        }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (BindingExist("Key"))
            {
                return;
            }

            Bindings.Add("Key", NamespaceFolderToken.Namespace);
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            var key = GetBinding<string>("Key");

            var existingNs = NamespaceFolderToken.Namespace;

            if (key == existingNs)
            {
                return;
            }

            using (var data = new DataConnection())
            {
                var resourceKeys = data.Get<IResourceKey>().Where(k => k.Key.StartsWith(existingNs + ".")).ToList();

                foreach (var resourceKey in resourceKeys)
                {
                    resourceKey.Key = resourceKey.Key.Remove(0, existingNs.Length).Insert(0, key);
                }

                data.Update<IResourceKey>(resourceKeys);

                var treeRefresher = CreateSpecificTreeRefresher();

                treeRefresher.PostRefreshMesseges(new LocalizationElementProviderEntityToken());
            }
        }
    }
}
