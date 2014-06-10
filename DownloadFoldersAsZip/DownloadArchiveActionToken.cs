using System.Collections.Generic;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    [ActionExecutor(typeof(DownloadActionExecutor))]
    public class DownloadArchiveActionToken : ActionToken
    {
        public string ArchiveId { get; private set; }

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return new[] { PermissionType.Edit, PermissionType.Administrate }; }
        }

        public override bool IgnoreEntityTokenLocking
        {
            get { return false; }
        }

        public DownloadArchiveActionToken(string archiveId)
        {
            ArchiveId = archiveId;
        }

        public override string Serialize()
        {
            return ArchiveId;
        }

        public static ActionToken Deserialize(string serialiedWorkflowActionToken)
        {
            return new DownloadArchiveActionToken(serialiedWorkflowActionToken);
        }
    }
}
