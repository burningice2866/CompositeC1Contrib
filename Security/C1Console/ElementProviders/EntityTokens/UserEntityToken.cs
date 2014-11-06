using System;
using System.Collections.Generic;
using System.Web.Security;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.Security.C1Console.ElementProviders.EntityTokens
{
    [SecurityAncestorProvider(typeof(UserAncestorProvider))]
    public class UserEntityToken : EntityToken
    {
        private readonly string _userName;
        public override string Id
        {
            get { return _userName; }
        }

        public override string Source
        {
            get { return String.Empty; }
        }

        public override string Type
        {
            get { return String.Empty; }
        }

        public UserEntityToken(MembershipUser user)
        {
            _userName = user.UserName;
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

            var user = Membership.GetUser(id);

            return new UserEntityToken(user);
        }
    }

    public class UserAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            var token = entityToken as UserEntityToken;
            if (token == null)
            {
                yield break;
            }

            var user = Membership.GetUser(token.Id);
            
            if (user.IsApproved)
            {
                yield return new FolderEntityToken("Approved");
            }

            if (!user.IsApproved)
            {
                yield return new FolderEntityToken("NotApproved");
            }
        }
    }
}
