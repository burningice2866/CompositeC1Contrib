using System;
using System.Collections.Generic;

using Composite.C1Console.Security;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Teasers.C1Console.EntityTokens
{
    [SecurityAncestorProvider(typeof(TeasersElementProviderAncestorProvider))]
    public class TeasersElementProviderEntityToken : EntityToken
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

        public override string Id
        {
            get { return "TeasersElementProviderEntityToken"; }
        }

        public IPage Page
        {
            get
            {
                var page = PageManager.GetPageById(new Guid(_source));

                return page;
            }
        }

        public TeasersElementProviderEntityToken(IPage page)
        {
            _source = page.Id.ToString();
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

            return page == null ? null : new TeasersElementProviderEntityToken(page);
        }
    }

    public class TeasersElementProviderAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            var token = entityToken as TeasersElementProviderEntityToken;
            if (token != null)
            {
                yield return token.Page.GetDataEntityToken();
            }
        }
    }
}
