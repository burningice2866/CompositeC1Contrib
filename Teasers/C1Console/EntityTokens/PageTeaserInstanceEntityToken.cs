using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Security;
using Composite.Core.Types;
using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.C1Console.EntityTokens
{
    [SecurityAncestorProvider(typeof(FormInstanceAncestorProvider))]
    public class PageTeaserInstanceEntityToken : EntityToken
    {
        private readonly string _type;
        public override string Type
        {
            get { return _type; }
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

        public IPageTeaser Teaser
        {
            get
            {
                var type = TypeManager.GetType(_type);
                var id = new Guid(_id);

                return DataFacade.GetData(type).Cast<IPageTeaser>().Single(t => t.Id == id);
            }
        }

        public PageTeaserInstanceEntityToken(IPage page, IPageTeaser teaser)
        {
            _source = page.Id.ToString();
            _type = TypeManager.SerializeType(teaser.DataSourceId.InterfaceType);
            _id = teaser.Id.ToString();
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
            var teaser = DataFacade.GetData(TypeManager.GetType(type)).Cast<IPageTeaser>().Single(t => t.Id == new Guid(id));

            return new PageTeaserInstanceEntityToken(page, teaser);
        }
    }

    public class FormInstanceAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            var teaserInstanceToken = entityToken as PageTeaserInstanceEntityToken;
            if (teaserInstanceToken != null)
            {
                var teaser = teaserInstanceToken.Teaser;

                yield return new PageTeaserPositionFolderEntityToken(teaserInstanceToken.Page, teaser.Position);
            }
        }
    }
}
