using System.Collections.Generic;

using Composite.C1Console.Security;

using Hangfire.Dashboard;

namespace CompositeC1Contrib.ScheduledTasks
{
    public class CompositeC1AuthorizationFilter : IAuthorizationFilter
    {
        public bool Authorize(IDictionary<string, object> owinEnvironment)
        {
            return UserValidationFacade.IsLoggedIn();
        }
    }
}
