using System;
using System.Collections.Generic;

using Composite.C1Console.Security;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Teasers.C1Console.EntityTokens
{
    [SecurityAncestorProvider(typeof(PageTeaserPositionFolderAncestorProvider))]
    public class PageTeaserPositionFolderEntityToken : EntityToken
    {
        public override string Type
        {
            get { return String.Empty; }
        }

        private readonly string _source;
        public override string Source
        {
            get { return _source; }
        }

        private readonly string _id;
        public override string Id
        {
            get { return _id; }
        }

        public IPage Page
        {
            get
            {
                var page = PageManager.GetPageById(new Guid(_source));

                return page;
            }
        }

        public PageTeaserPositionFolderEntityToken(IPage page, string position)
        {
            _source = page.Id.ToString();
            _id = position;
        }

        public override string Serialize()
        {
            return DoSerialize();
        }

        public static EntityToken Deserialize(string serializedEntityToken)
        {
            string type;
            string source;
            string id;

            DoDeserialize(serializedEntityToken, out type, out source, out id);

            var page = PageManager.GetPageById(new Guid(source));

            return page == null ? null : new PageTeaserPositionFolderEntityToken(page, id);
        }
    }

    public class PageTeaserPositionFolderAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            var folderToken = entityToken as PageTeaserPositionFolderEntityToken;
            if (folderToken != null)
            {
                yield return new TeasersElementProviderEntityToken(folderToken.Page);
            }
        }
    }
}
