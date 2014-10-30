using System;
using System.Linq;
using System.Web.UI;

using Composite.Core.PackageSystem;

namespace CompositeC1Contrib.SiteUpdate.Web.UI
{
    public class BasePage : Page
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
            get { return String.Format("?consoleId={0}&EntityToken={1}", ConsoleId, EntityToken); }
        }

        public string InstalledInformation(SiteUpdateInformation update)
        {
            var installedPackage = PackageManager.GetInstalledPackages().SingleOrDefault(p => p.Id == update.Id);

            return installedPackage == null ? "No" : installedPackage.InstallDate.ToString("G") + " by " + installedPackage.InstalledBy;
        }
    }
}
