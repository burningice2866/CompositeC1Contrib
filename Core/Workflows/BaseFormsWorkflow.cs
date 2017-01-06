using System.Globalization;
using System.Linq;
using System.Threading;

using Composite.C1Console.Security;
using Composite.C1Console.Users;
using Composite.C1Console.Workflow.Activities;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Workflows
{
    public abstract class BaseFormsWorkflow : FormsWorkflow
    {
        protected void SetCultureInfo()
        {
            if (UserValidationFacade.IsLoggedIn())
            {
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = UserSettings.ActiveLocaleCultureInfo;
            }
            else
            {
                using (var data = new DataConnection())
                {
                    var activeLocales = data.Get<ISystemActiveLocale>().ToList();
                    if (activeLocales.Count == 1)
                    {
                        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo(activeLocales[0].CultureName);
                    }
                }
            }
        }
    }
}
