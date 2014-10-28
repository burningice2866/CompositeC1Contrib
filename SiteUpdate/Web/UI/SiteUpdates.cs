using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Composite.Core.Configuration;
using Composite.Core.PackageSystem;

using CompositeC1Contrib.SiteUpdate.Configuration;

namespace CompositeC1Contrib.SiteUpdate.Web.UI
{
    public class SiteUpdates : Page
    {
        private SiteUpdateStore _store;
        private IEnumerable<SiteUpdateInformation> _updates;

        protected Repeater rptUpdate;

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

        protected override void OnInit(EventArgs e)
        {
            var location = SiteUpdateConfigurationSection.GetSection().StoreLocation;
            var installationId = InstallationInformationFacade.InstallationId;

            _store = new SiteUpdateStore(location);
            _updates = _store.GetUpdateSummaries(installationId);

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!IsPostBack)
            {
                var updateId = Request.QueryString["package"];

                var cmd = Request.QueryString["cmd"];
                if (cmd == "install")
                {
                    var update = _updates.Single(u => u.Id == Guid.Parse(updateId));

                    using (var zip = _store.GetZipStream(update))
                    {
                        var installProcess = PackageManager.Install(zip, true);

                        var validatationResult = installProcess.Validate();
                        if (validatationResult.Any())
                        {
                            throw new Exception(validatationResult.First().Message);
                        }

                        installProcess.Install();
                    }
                }

                if (cmd == "uninstall")
                {
                    var uninstallProcess = PackageManager.Uninstall(Guid.Parse(updateId));

                    var validatationResult = uninstallProcess.Validate();
                    if (validatationResult.Any())
                    {
                        throw new Exception(validatationResult.First().Message);
                    }

                    uninstallProcess.Uninstall();
                }

                Bind();
            }

            base.OnLoad(e);
        }

        private void Bind()
        {
            rptUpdate.DataSource = _updates;
            rptUpdate.DataBind();
        }

        public bool IsInstalled(SiteUpdateInformation update)
        {
            var installedPackage = PackageManager.GetInstalledPackages().SingleOrDefault(p => p.Id == update.Id);

            return installedPackage != null;
        }

        public string InstalledInformation(SiteUpdateInformation update)
        {
            var installedPackage = PackageManager.GetInstalledPackages().SingleOrDefault(p => p.Id == update.Id);

            return installedPackage == null ? "No" : installedPackage.InstallDate.ToString("G");
        }
    }
}
