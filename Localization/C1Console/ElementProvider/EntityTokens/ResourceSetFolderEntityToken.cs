using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;

namespace CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens
{
    [SecurityAncestorProvider(typeof(ResourceSetFolderAncestorProvider))]
    public class ResourceSetFolderEntityToken : LocalizationEntityToken
    {
        public ResourceSetFolderEntityToken(string resourceSet) : base(resourceSet) { }

        public static Element CreateElement(ElementProviderContext context, string label, string resourceSet, string ns)
        {
            var folderHandle = context.CreateElementHandle(new NamespaceFolderEntityToken(resourceSet, ns));
            var folderElement = new Element(folderHandle)
            {
                VisualData = new ElementVisualizedData
                {
                    Label = label,
                    ToolTip = label,
                    HasChildren = true,
                    Icon = ResourceHandle.BuildIconFromDefaultProvider("datagroupinghelper-folder-closed"),
                    OpenedIcon = ResourceHandle.BuildIconFromDefaultProvider("datagroupinghelper-folder-open")
                }
            };

            return folderElement;
        }

        public override string Serialize()
        {
            return DoSerialize();
        }

        public static EntityToken Deserialize(string serializedEntityToken)
        {

            DoDeserialize(serializedEntityToken, out string _, out string _, out string id);

            return new ResourceSetFolderEntityToken(id);
        }
    }

    public class ResourceSetFolderAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            var resourseSetToken = entityToken as ResourceSetFolderEntityToken;
            if (resourseSetToken == null)
            {
                return Enumerable.Empty<EntityToken>();
            }

            return new[] { new LocalizationElementProviderEntityToken(resourseSetToken.ResourceSet) };
        }
    }
}
