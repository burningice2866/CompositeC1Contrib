using System;
using System.Linq;

using Composite.Core.Configuration;

using CompositeC1Contrib.SiteUpdate.Configuration;

namespace CompositeC1Contrib.SiteUpdate.Web.UI
{
    public class Details : BasePage
    {
        protected SiteUpdateInformation Update { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            var location = SiteUpdateConfigurationSection.GetSection().StoreLocation;
            var store = new SiteUpdateStore(location);
            var installationId = InstallationInformationFacade.InstallationId;
            var updates = store.GetUpdateSummaries(installationId);

            Update = updates.Single(u => u.Id == Guid.Parse(Request.QueryString["package"]));

            base.OnInit(e);
        }
    }
}
