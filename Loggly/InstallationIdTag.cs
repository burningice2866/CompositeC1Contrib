using Composite.Core.Configuration;

using Loggly;

namespace CompositeC1Contrib.Loggly
{
    public class InstallationIdTag : ComplexTag
    {
        public override string InputValue
        {
            get { return InstallationInformationFacade.InstallationId.ToString(); }
        }
    }
}
