using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Security;
using Composite.C1Console.Users;
using Composite.Data;

using CompositeC1Contrib.Localization.C1Console.Actions;
using CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens;
using CompositeC1Contrib.Workflows;

namespace CompositeC1Contrib.Localization.C1Console.Workflows
{
    public sealed class ExportWorkflow : Basic1StepDialogWorkflow
    {
        private string Namespace
        {
            get
            {
                var ns = String.Empty;

                var nsToken = EntityToken as NamespaceFolderEntityToken;
                if (nsToken != null)
                {
                    ns = nsToken.Namespace;
                }

                return ns;
            }
        }

        public ExportWorkflow() : base("\\InstalledPackages\\CompositeC1Contrib.Localization\\Export.xml") { }

        public static IDictionary<string, string> GetLanguages()
        {
            var ret = new Dictionary<string, string>();

            var isAdministratior = PermissionsFacade.IsAdministrator(UserSettings.Username);
            var userLocales = UserSettings.GetActiveLocaleCultureInfos(UserSettings.Username).ToList();

            foreach (var culture in DataLocalizationFacade.ActiveLocalizationCultures)
            {
                if (isAdministratior || userLocales.Contains(culture))
                {
                    ret.Add(culture.Name, culture.DisplayName);
                }
            }

            return ret;
        }

        public static IDictionary<string, string> GetResourceSets()
        {
            var ret = new Dictionary<string, string>() { };

            using (var data = new DataConnection())
            {
                var query = (from key in data.Get<IResourceKey>()
                             select key.ResourceSet).Distinct();

                foreach (var q in query)
                {
                    if (q == null)
                    {
                        ret.Add(String.Empty, "Website");
                    }
                    else
                    {
                        ret.Add(q, q);
                    }
                }
            }

            return ret;
        }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (BindingExist("Namespace"))
            {
                return;
            }

            Bindings.Add("Namespace", Namespace);
            Bindings.Add("Languages", GetLanguages().Keys.ToList());
            Bindings.Add("ResourceSets", GetResourceSets().Keys.ToList());
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            var languages = GetBinding<List<string>>("Languages");
            var resourceSets = GetBinding<List<string>>("ResourceSets");

            var actionToken = new DownloadExportedResourcesActionToken(languages.ToArray(), resourceSets.ToArray(), Namespace);

            ExecuteAction(EntityToken, actionToken);
        }
    }
}
