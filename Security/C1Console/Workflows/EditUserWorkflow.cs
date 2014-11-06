using System;
using System.Web.Security;

using CompositeC1Contrib.Email;
using CompositeC1Contrib.Workflows;

namespace CompositeC1Contrib.Security.C1Console.Workflows
{
    public class EditUserWorkflow : Basic1StepDocumentWorkflow
    {
        public EditUserWorkflow() : base("\\InstalledPackages\\CompositeC1Contrib.Security\\EditUserWorkflow.xml") { }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (BindingExist("UserName"))
            {
                return;
            }

            var user = Membership.GetUser(EntityToken.Id);

            Bindings.Add("UserName", user.UserName);
            Bindings.Add("Email", user.Email);

            Bindings.Add("CreationDate", user.CreationDate.ToLocalTime().ToLongDateString());
            Bindings.Add("LastActivityDate", user.LastActivityDate);
            Bindings.Add("LastLockoutDate", user.LastLockoutDate);
            Bindings.Add("LastLoginDate", user.LastLoginDate);
            Bindings.Add("LastPasswordChangedDate", user.LastPasswordChangedDate);
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            var user = Membership.GetUser(EntityToken.Id);
            var email = GetBinding<string>("Email");

            if (email != user.Email)
            {
                user.Email = email;

                Membership.UpdateUser(user);
            }

            CreateSpecificTreeRefresher().PostRefreshMesseges(EntityToken);
            SetSaveStatus(true);
        }

        public override bool Validate()
        {
            var user = Membership.GetUser(EntityToken.Id);
            var userName = GetBinding<string>("UserName");
            var email = GetBinding<string>("Email");

            if (String.IsNullOrEmpty(email))
            {
                ShowFieldMessage("Email", "Email required");

                return false;
            }

            if (MailsFacade.ValidateMailAddress(userName))
            {
                ShowFieldMessage("Email", "Provided email is not valid");

                return false;
            }

            if (email != user.Email)
            {
                if (Membership.FindUsersByEmail(email).Count != 0)
                {
                    ShowFieldMessage("email", "Email already exists");

                    return false;
                }
            }

            return base.Validate();
        }
    }
}
