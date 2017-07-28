using System;

using Composite.C1Console.Users;

using CompositeC1Contrib.Localization.C1Console.Actions;
using CompositeC1Contrib.Workflows;

namespace CompositeC1Contrib.Localization.C1Console.Workflows
{
    public sealed class GenerateClassWithKeysWorkflow : Basic1StepDialogWorkflow
    {
        public GenerateClassWithKeysWorkflow() : base("\\InstalledPackages\\CompositeC1Contrib.Localization\\GenerateClassWithKeys.xml") { }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (BindingExist("Namespace"))
            {
                return;
            }

            Bindings.Add("Namespace", UserSettings.LastSpecifiedNamespace);
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            var ns = GetBinding<string>("Namespace");

            if (!UserSettings.LastSpecifiedNamespace.Equals(ns, StringComparison.InvariantCulture))
            {
                UserSettings.LastSpecifiedNamespace = ns;
            }

            CloseCurrentView();
            ExecuteAction(EntityToken, new GenerateClassWithKeysActionToken(ns));
        }
    }
}
