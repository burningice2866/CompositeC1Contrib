using System.Collections.Generic;

using Composite.C1Console.Security;
using Composite.C1Console.Actions;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    [ActionExecutor(typeof(DownloadActionExecutor))]
    public class DownloadFileFolderActionToken : ActionToken
    {
        public string Path { get; private set; }

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return new[] { PermissionType.Edit, PermissionType.Administrate }; }
        }

        public override bool IgnoreEntityTokenLocking
        {
            get { return false; }
        }

        public DownloadFileFolderActionToken(string path)
        {
            Path = path;
        }

        public override string Serialize()
        {
            return Path;
        }

        public static ActionToken Deserialize(string serialiedWorkflowActionToken)
        {
            return new DownloadFileFolderActionToken(serialiedWorkflowActionToken);
        }
    }
}
