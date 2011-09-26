using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Composite.C1Console.Security;
using Composite.C1Console.Actions;

namespace CompositeC1Contrib.Email.ElementProviders
{
    [ActionExecutor(typeof(ToggleEmailQueueStateActionExecutor))]
    public class ToggleEmailQueueStateActionToken : ActionToken
    {
        private Guid _queueId;
        public Guid QueueId
        {
            get { return _queueId; }
        }

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return new[] { PermissionType.Edit, PermissionType.Administrate }; }
        }

        public override bool IgnoreEntityTokenLocking
        {
            get { return false; }
        }

        public ToggleEmailQueueStateActionToken(Guid queueId)
        {
            _queueId = queueId;
        }

        public override string Serialize()
        {
            return _queueId.ToString();
        }

        public static ActionToken Deserialize(string serialiedWorkflowActionToken)
        {
            return new ToggleEmailQueueStateActionToken(Guid.Parse(serialiedWorkflowActionToken));
        }
    }
}
