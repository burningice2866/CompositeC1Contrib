using System;
using System.Reflection;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.Web.Security
{
    public static class LoginProviderHelper
    {
        private static Type _loginFacade = Type.GetType("Composite.C1Console.Security.Foundation.PluginFacades.LoginProviderPluginFacade, Composite");
        private static Type _dbLoginProvider = Type.GetType("Composite.Plugins.Security.LoginProviderPlugins.DataBasedFormLoginProvider.DataBasedFormLoginProvider, Composite");

        public static bool CanAddNewUser;
        public static bool IsBuiltInDbProvider;

        static LoginProviderHelper()
        {
            CanAddNewUser = (bool)_loginFacade.GetProperty("CanAddNewUser", BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
            IsBuiltInDbProvider = ((Type)_loginFacade.GetMethod("GetValidationPluginType", BindingFlags.Static | BindingFlags.Public).Invoke(null, null)).Equals(_dbLoginProvider);
        }

        public static bool AddNewUser(string username, string password)
        {
            try
            {
                _loginFacade.GetMethod("FormAddNewUser", BindingFlags.Static | BindingFlags.Public).Invoke(null, new[] { username, password, String.Empty });

                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public static bool Validate(string username, string password)
        {
            var validationType = UserValidationFacade.GetValidationType();
            switch (validationType)
            {
                case UserValidationFacade.ValidationType.Form: 
                    return UserValidationFacade.FormValidateUserWithoutLogin(username, password);

                case UserValidationFacade.ValidationType.Windows: 
                    return (bool)_loginFacade.GetMethod("WindowsValidateUser", BindingFlags.Static | BindingFlags.Public).Invoke(null, new[] { username, password });
            }

            return false;
        }
    }
}