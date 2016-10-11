using Composite.C1Console.Security;

using Hangfire.Dashboard;

namespace CompositeC1Contrib.ScheduledTasks
{
    public class CompositeC1AuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return UserValidationFacade.IsLoggedIn();
        }
    }
}
