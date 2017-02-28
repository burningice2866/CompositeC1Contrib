using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Data;
using Composite.Functions;

using CompositeC1Contrib.Composition;

namespace CompositeC1Contrib.FunctionRoutes
{
    [Export(typeof(IElementActionProviderFor))]
    public class DataActionProvider : IElementActionProviderFor
    {
        public IEnumerable<Type> ProviderFor => new[] { typeof(EntityToken) };

        public void AddActions(Element element)
        {
            var actions = Provide(element.ElementHandle.EntityToken);

            element.AddAction(actions);
        }

        public IEnumerable<ElementAction> Provide(EntityToken entityToken)
        {
            var functionName = entityToken.Id;
            if (!functionName.Contains("."))
            {
                yield break;
            }

            IFunction function;
            if (!FunctionFacade.TryGetFunction(out function, functionName))
            {
                yield break;
            }

            var message = (string)null;
            var icon = (string)null;

            using (var data = new DataConnection())
            {
                var route = data.Get<IFunctionRoute>().SingleOrDefault(r => r.Function == functionName);
                if (route == null)
                {
                    message = "Enable Function Route";
                    icon = "accept";
                }
                else
                {
                    message = "Disable Function Route";
                    icon = "delete";
                }
            }

            var actionToken = new ToggleFunctionRouteActionToken(functionName);

            yield return new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = message,
                    ToolTip = message,
                    Icon = new ResourceHandle("Composite.Icons", icon),
                    ActionLocation = ActionLocation.OtherPrimaryActionLocation
                }
            };
        }
    }
}
