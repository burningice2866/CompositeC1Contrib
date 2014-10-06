using System;
using System.Linq;
using System.Web.UI;

using Composite.Core.Configuration;

using CompositeC1Contrib.SiteUpdate.Configuration;

namespace CompositeC1Contrib.SiteUpdate.Web.UI
{
    public class Details : Page
    {
        protected string EntityToken
        {
            get { return Request.QueryString["EntityToken"]; }
        }

        protected string ConsoleId
        {
            get { return Request.QueryString["consoleId"]; }
        }

        protected string BaseUrl
        {
            get
            {
                var qs = Request.QueryString;

                return String.Format("?consoleId={0}&EntityToken={1}", ConsoleId, EntityToken);
            }
        }

        private SiteUpdateInformation _update;
        protected SiteUpdateInformation Update {
            get { return _update; }
        }

        protected override void OnInit(EventArgs e)
        {
            var location = SiteUpdateConfigurationSection.GetSection().StoreLocation;
            var store = new SiteUpdateStore(location);
            var installationId = InstallationInformationFacade.InstallationId;
            var updates = store.GetUpdateSummaries(installationId);

            _update = updates.Single(u => u.Id == Guid.Parse(Request.QueryString["id"]));

            base.OnInit(e);
        }
    }
}
