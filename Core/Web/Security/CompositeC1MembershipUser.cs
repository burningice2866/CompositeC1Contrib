using System;
using System.Linq;
using System.Web.Security;

using Composite.C1Console.Security;
using Composite.C1Console.Security.Cryptography;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web.Security
{
    public class CompositeC1MembershipUser : MembershipUser
    {
         private CompositeC1MembershipUser(MembershipProvider provider, IUser user)
            : base(provider.Name, 
            user.Username, user.Id, user.Email,
            String.Empty, String.Empty,
            true, false, 
            DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue)
        {
            using (var data = new DataConnection())
            {
                var _membershipUser = data.Get<IMembershipUser>().SingleOrDefault(u => u.Id == user.Id);
                if (_membershipUser == null)
                {
                    _membershipUser = data.CreateNew<IMembershipUser>();
                    _membershipUser.Id = user.Id;
                    _membershipUser.CreationDate = DateTime.Now;

                    data.Add(_membershipUser);
                }

                LastActivityDate = _membershipUser.LastActivityDate;
                LastLoginDate = _membershipUser.LastLoginDate;
            }            
        }

        public override bool ChangePassword(string oldPassword, string newPassword)
        {
            if (UserValidationFacade.CanSetUserPassword)
            {
                UserValidationFacade.FormSetUserPassword(UserName, newPassword);

                return true;
            }

            return false;
        }

        public override string GetPassword()
        {
            if (LoginProviderHelper.IsBuiltInDbProvider)
            {
                var user = DataFacade.GetData<IUser>().Single(u => u.Id == (Guid)ProviderUserKey);

                return user.EncryptedPassword.Decrypt();
            }

            return String.Empty;
        }

        public static MembershipUser CreateUser(MembershipProvider provider, string username, string email, string password)
        {
            if (LoginProviderHelper.CanAddNewUser)
            {
                LoginProviderHelper.AddNewUser(username, password);

                var user = DataFacade.GetData<IUser>().Single(u => u.Username == username);
                return Wrap(provider, user);
            }

            return null;            
        }

        public static bool DeleteUser(MembershipUser user)
        {
            using (var data = new DataConnection())
            {
                var iUser = data.Get<IUser>().Single(u => u.Id == (Guid)user.ProviderUserKey);
                var iMembershipUser = data.Get<IMembershipUser>().Single(u => u.Id == iUser.Id);

                if (DataFacade.WillDeleteSucceed(iUser) && DataFacade.WillDeleteSucceed(iMembershipUser))
                {
                    UserPerspectiveFacade.DeleteAll(iUser.Username);

                    data.Delete(iUser);
                    data.Delete(iMembershipUser);

                    return true;
                }
            }

            return false;            
        }

        public static MembershipUser Wrap(MembershipProvider provider, IUser user)
        {
            return new CompositeC1MembershipUser(provider, user);
        }
    }
}