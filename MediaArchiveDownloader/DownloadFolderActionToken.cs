using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.MediaArchiveDownloader
{
    [ActionExecutor(typeof(DownloadActionExecutor))]
    public class DownloadFolderActionToken : ActionToken
    {
        private IMediaFileFolder _folder;
        public IMediaFileFolder MediaFileFolder
        {
            get { return _folder; }
        }

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return new[] { PermissionType.Edit, PermissionType.Administrate }; }
        }

        public override bool IgnoreEntityTokenLocking
        {
            get { return false; }
        }

        public DownloadFolderActionToken(IMediaFileFolder folder)
        {
            _folder = folder;
        }

        public override string Serialize()
        {
            return _folder.KeyPath;
        }

        public static ActionToken Deserialize(string serialiedWorkflowActionToken)
        {
            using (var data = new DataConnection())
            {
                var folder = data.Get<IMediaFileFolder>().Single(f => f.KeyPath == serialiedWorkflowActionToken);
            
                return new DownloadFolderActionToken(folder);
            }
        }
    }
}
