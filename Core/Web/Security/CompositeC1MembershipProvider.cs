using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Security;

using Composite;
using Composite.C1Console.Security;
using Composite.Data;
using Composite.Data.DynamicTypes;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web.Security
{
    public class CompositeC1MembershipProvider : MembershipProvider
    {
        public override int MaxInvalidPasswordAttempts
        {
            get { return int.MaxValue; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return 0; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 1; }
        }

        public override int PasswordAttemptWindow
        {
            get { return int.MaxValue; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                if (LoginProviderHelper.IsBuiltInDbProvider)
                {
                    return MembershipPasswordFormat.Encrypted;
                }

                return MembershipPasswordFormat.Clear;
            }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return false; }
        }

        public override bool EnablePasswordReset
        {
            get { return false; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override string ApplicationName
        {
            get { return RuntimeInformation.UniqueInstanceName; }
            set { throw new NotSupportedException("Provider doesn't support this method"); }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var user = GetUser(username, false);

            return user.ChangePassword(oldPassword, newPassword);
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            MembershipUser user = null;
            status = MembershipCreateStatus.ProviderError;

            user = CompositeC1MembershipUser.CreateUser(this, username, email, password);
            if (user != null)
            {
                status = MembershipCreateStatus.Success;
            }            

            return user;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            var user = GetUser(username, false);

            return CompositeC1MembershipUser.DeleteUser(user);
        }        

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException("Can only search by username");
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var collection = new MembershipUserCollection();
            var allUserNames = UserValidationFacade.AllUsernames.Where(s => s.StartsWith(usernameToMatch));

            totalRecords = allUserNames.Count();

            var page = allUserNames.Skip(pageIndex * pageSize).Take(pageSize);
            foreach (var userName in page)
            {
                var user = GetUser(userName, false);
                collection.Add(user);
            }

            return collection;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var collection = new MembershipUserCollection();
            var allUserNames = UserValidationFacade.AllUsernames;

            totalRecords = allUserNames.Count();

            var page = allUserNames.Skip(pageIndex * pageSize).Take(pageSize);
            foreach (var userName in page)
            {
                var user = GetUser(userName, false);
                collection.Add(user);
            }

            return collection;
        }

        public override int GetNumberOfUsersOnline()
        {
            return 0;
        }

        public override string GetPassword(string username, string answer)
        {
            return GetUser(username, false).GetPassword();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (UserValidationFacade.AllUsernames.Contains(username))
            {
                var user = DataFacade.GetData<IUser>().SingleOrDefault(u => u.Username == username);
                if (user != null)
                {
                    var membershipUser = CompositeC1MembershipUser.Wrap(this, user);

                    if (userIsOnline)
                    {
                        membershipUser.LastActivityDate = DateTime.Now.ToUniversalTime();
                        UpdateUser(membershipUser);
                    }

                    return membershipUser;
                }
            }

            return null;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var user = DataFacade.GetData<IUser>().SingleOrDefault(u => u.Id == (Guid)providerUserKey);
            if (user != null && UserValidationFacade.AllUsernames.Contains(user.Username))
            {
                var membershipUser = CompositeC1MembershipUser.Wrap(this, user);

                if (userIsOnline)
                {
                    membershipUser.LastActivityDate = DateTime.Now.ToUniversalTime();
                    UpdateUser(membershipUser);
                }

                return membershipUser;
            }

            return null;
        }

        public override string GetUserNameByEmail(string email)
        {
            var user = DataFacade.GetData<IUser>().SingleOrDefault(u => u.Email == email);
            if (user != null && UserValidationFacade.AllUsernames.Contains(user.Username))
            {
                return user.Username;
            }

            return String.Empty;
        }        

        public override string ResetPassword(string username, string answer)
        {
            throw new NotSupportedException("No password retrieval or reset service implemented");
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotSupportedException("C1 users cannot be locked");
        }

        public override void UpdateUser(MembershipUser user)
        {
            using (var data = new DataConnection())
            {
                var iUser = data.Get<IUser>().Single(u => u.Id == (Guid)user.ProviderUserKey);
                var iMembershipUser = data.Get<IMembershipUser>().Single(u => u.Id == iUser.Id);

                iUser.Username = user.UserName;
                iUser.Email = user.Email;

                iMembershipUser.LastActivityDate = user.LastActivityDate;
                iMembershipUser.LastLoginDate = user.LastLoginDate;
                iMembershipUser.Comment = user.Comment;
                iMembershipUser.LastLockoutDate = user.LastLockoutDate;

                data.Update(iUser);
                data.Update(iMembershipUser);

            }            
        }

        public override bool ValidateUser(string username, string password)
        {
            bool validated = UserValidationFacade.AllUsernames.Contains(username) && LoginProviderHelper.Validate(username, password);
            if (validated)
            {
                var user = DataFacade.GetData<IUser>().Single(u => u.Username == username);
                var membershipUser = CompositeC1MembershipUser.Wrap(this, user);

                membershipUser.LastActivityDate = DateTime.Now.ToUniversalTime();
                membershipUser.LastLoginDate = membershipUser.LastActivityDate;
                UpdateUser(membershipUser);
            }

            return validated;
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            DynamicTypeManager.EnsureCreateStore(typeof(IMembershipUser));

            base.Initialize(name, config);
        }
    }
}