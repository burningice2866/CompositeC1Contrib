using System.Collections.Generic;

using Composite.C1Console.Security;
using Composite.C1Console.Actions;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    [ActionExecutor(typeof(DownloadActionExecutor))]
    public class DownloadActionToken : ActionToken
    {
        public string Type { get; private set; }
        public string Path { get; private set; }

        public override IEnumerable<PermissionType> PermissionTypes
        {
            get { return new[] { PermissionType.Edit, PermissionType.Administrate }; }
        }

        public override bool IgnoreEntityTokenLocking
        {
            get { return false; }
        }

        public DownloadActionToken(string type, string path)
        {
            Type = type;
            Path = path;
        }

        public override string Serialize()
        {
            return Type + "·" + Path + "·";
        }

        public static ActionToken Deserialize(string serializedData)
        {
            var s = serializedData.Split('·');

            return new DownloadActionToken(s[0], s[1]);
        }
    }
}
