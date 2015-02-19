using System.Collections.Generic;
using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Data;

using Composite.Data.Types;

namespace CompositeC1Contrib.GetItemLink
{
    [ActionExecutor(typeof(GetLinkActionExecutor))]
    public class GetPageLinkActionToken : ActionToken
    {
        public IPage Page { get; private set; }

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return new[] { PermissionType.Edit, PermissionType.Administrate }; }
        }

        public override bool IgnoreEntityTokenLocking
        {
            get { return false; }
        }

        public GetPageLinkActionToken(IPage page)
        {
            Page = page;
        }

        public override string Serialize()
        {
            return EntityTokenSerializer.Serialize(Page.GetDataEntityToken());
        }

        public static ActionToken Deserialize(string serialiedWorkflowActionToken)
        {
            var data = (DataEntityToken)EntityTokenSerializer.Deserialize(serialiedWorkflowActionToken);
            var page = (IPage)data.Data;

            return new GetPageLinkActionToken(page);
        }
    }
}
