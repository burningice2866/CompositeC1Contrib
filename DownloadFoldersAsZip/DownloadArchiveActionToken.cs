using System.Collections.Generic;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    [ActionExecutor(typeof(DownloadActionExecutor))]
    public class DownloadArchiveActionToken : ActionToken
    {
        private string _archiveId;
        public string ArchiveId
        {
            get { return _archiveId; }
        }

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
            _archiveId = archiveId;
        }

        public override string Serialize()
        {
            return _archiveId;
        }

        public static ActionToken Deserialize(string serialiedWorkflowActionToken)
        {
            return new DownloadArchiveActionToken(serialiedWorkflowActionToken);
        }
    }
}
