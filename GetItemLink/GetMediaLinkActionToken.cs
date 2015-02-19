using System.Collections.Generic;
using Composite.C1Console.Actions;
using Composite.C1Console.Events;
using Composite.C1Console.Security;
using Composite.Core.WebClient;
using Composite.Data;

using Composite.Data.Types;

namespace CompositeC1Contrib.GetItemLink
{
    [ActionExecutor(typeof(GetLinkActionExecutor))]
    public class GetMediaLinkActionToken : ActionToken
    {
        public IMediaFile File { get; private set; }

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return new[] { PermissionType.Edit, PermissionType.Administrate }; }
        }

        public override bool IgnoreEntityTokenLocking
        {
            get { return false; }
        }

        public GetMediaLinkActionToken(IMediaFile file)
        {
            File = file;
        }

        public override string Serialize()
        {
            return EntityTokenSerializer.Serialize(File.GetDataEntityToken());
        }

        public static ActionToken Deserialize(string serialiedWorkflowActionToken)
        {
            var data = (DataEntityToken)EntityTokenSerializer.Deserialize(serialiedWorkflowActionToken);
            var file = (IMediaFile)data.Data;

            return new GetMediaLinkActionToken(file);
        }
    }
}
