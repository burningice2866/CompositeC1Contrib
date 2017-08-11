using System;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens
{
    public abstract class LocalizationEntityToken : EntityToken
    {
        public override string Type => String.Empty;

        public override string Source => ResourceSet;

        public override string Id => String.Empty;

        public string ResourceSet { get; }

        protected LocalizationEntityToken(string resourceSet)
        {
            ResourceSet = resourceSet ?? String.Empty;
        }
    }
}
