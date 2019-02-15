using System;

using Composite.C1Console.Security;
using Composite.C1Console.Security.SecurityAncestorProviders;

namespace CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.EntityTokens
{
    [SecurityAncestorProvider(typeof(NoAncestorSecurityAncestorProvider))]
    public class ScheduledTasksElementProviderEntityToken : EntityToken
    {
        public override string Id => "MailElementProviderEntityToken";

        public override string Source => String.Empty;

        public override string Type => String.Empty;

        public static EntityToken Deserialize(string serializedData)
        {
            return new ScheduledTasksElementProviderEntityToken();
        }

        public override string Serialize()
        {
            return String.Empty;
        }
    }
}
