using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Data;

namespace CompositeC1Contrib.FunctionRoutes
{
    [ActionExecutor(typeof(ToggleFunctionRouteActionExecutor))]
    public class ToggleFunctionRouteActionToken : ActionToken
    {
        public string Function { get; }

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return new[] { PermissionType.Edit, PermissionType.Administrate }; }
        }

        public override bool IgnoreEntityTokenLocking
        {
            get { return false; }
        }

        public ToggleFunctionRouteActionToken(string function)
        {
            Function = function;
        }

        public override string Serialize()
        {
            return Function;
        }

        public static ActionToken Deserialize(string serialiedWorkflowActionToken)
        {
            return new ToggleFunctionRouteActionToken(serialiedWorkflowActionToken);
        }
    }

    public class ToggleFunctionRouteActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var token = (ToggleFunctionRouteActionToken)actionToken;

            using (var data = new DataConnection())
            {
                var route = data.Get<IFunctionRoute>().SingleOrDefault(r => r.Function == token.Function);
                if (route == null)
                {
                    route = data.CreateNew<IFunctionRoute>();

                    route.Id = Guid.NewGuid();
                    route.Function = token.Function;

                    data.Add(route);
                }
                else
                {
                    data.Delete(route);
                }
            }

            var treeRefresher = new ParentTreeRefresher(flowControllerServicesContainer);
            treeRefresher.PostRefreshMesseges(entityToken);

            return null;
        }
    }
}
