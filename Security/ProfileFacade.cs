using System.Web.Security;

namespace CompositeC1Contrib.Security
{
    public class ProfileFacade
    {
        private static IProfileResolver _resolver;

        public static void RegisterResolver(IProfileResolver resolver)
        {
            _resolver = resolver;
        }

        public static T GetProfile<T>()
        {
            var user = Membership.GetUser();

            return (T)_resolver.Resolve(user);
        }

        public static T GetProfileForUser<T>(MembershipUser user)
        {
            return (T)_resolver.Resolve(user);
        }
    }
}
