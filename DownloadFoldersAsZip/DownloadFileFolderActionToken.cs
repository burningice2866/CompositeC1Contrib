using System.Collections.Generic;

using Composite.C1Console.Security;
using Composite.C1Console.Actions;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    [ActionExecutor(typeof(DownloadActionExecutor))]
    public class DownloadFileFolderActionToken : ActionToken
    {
        private string _path;
        public string Path
        {
            get { return _path; }
        }

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
            _path = path;
        }

        public override string Serialize()
        {
            return _path;
        }

        public static ActionToken Deserialize(string serialiedWorkflowActionToken)
        {
            return new DownloadFileFolderActionToken(serialiedWorkflowActionToken);
        }
    }
}
