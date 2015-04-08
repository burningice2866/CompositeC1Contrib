using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Composite.Core.Configuration;
using Composite.Core.PackageSystem;

using CompositeC1Contrib.SiteUpdate.Configuration;

namespace CompositeC1Contrib.SiteUpdate.Web.UI
{
    public class SiteUpdates : BasePage
    {
        private SiteUpdateStore _store;
        private IEnumerable<SiteUpdateInformation> _updates;

        protected PlaceHolder plcErrors;
        protected Repeater rptUpdate;

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
                Guid updateId;
                if (Guid.TryParse(Request.QueryString["package"], out updateId))
                {
                    var update = _updates.Single(u => u.Id == updateId);

                    var cmd = Request.QueryString["cmd"];
                    if (cmd == "install")
                    {
                        using (var zip = _store.GetZipStream(update))
                        {
                            var installProcess = PackageManager.Install(zip, true);

                            if (installProcess.PreInstallValidationResult.Any())
                            {
                                HandleErrors(update, installProcess.PreInstallValidationResult);
                            }
                            else
                            {
                                var validatationResult = installProcess.Validate();
                                if (validatationResult.Any())
                                {
                                    HandleErrors(update, validatationResult);
                                }
                                else
                                {
                                    installProcess.Install();
                                }
                            }
                        }
                    }

                    if (cmd == "uninstall")
                    {
                        var uninstallProcess = PackageManager.Uninstall(update.Id);

                        if (uninstallProcess.PreUninstallValidationResult.Any())
                        {
                            HandleErrors(update, uninstallProcess.PreUninstallValidationResult);
                        }
                        else
                        {
                            var validatationResult = uninstallProcess.Validate();
                            if (validatationResult.Any())
                            {
                                HandleErrors(update, validatationResult);
                            }
                            else
                            {
                                uninstallProcess.Uninstall();
                            }
                        }
                    }
                }

                Bind();
            }

            base.OnLoad(e);
        }

        private void HandleErrors(SiteUpdateInformation update, IEnumerable<PackageFragmentValidationResult> validatationResult)
        {
            var sb = new StringBuilder();

            sb.Append("<h2>Error</h2>");
            sb.AppendFormat("<p>There was an error processing the request for {0}</p>", update.Name);
            sb.Append("<ul>");

            foreach (var itm in validatationResult)
            {
                sb.AppendFormat("<li>{0}</li>", itm.Message);
            }

            sb.Append("</ul>");

            plcErrors.Controls.Add(new LiteralControl(sb.ToString()));
            plcErrors.Visible = true;
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
    }
}
