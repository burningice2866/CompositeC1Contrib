using System;
using System.Collections.Generic;
using System.Linq;

namespace CompositeC1Contrib.Localization.C1Console
{
    public static class FormUtils
    {
        public static IDictionary<string, string> GetResourceKeyTypes()
        {
            return Enum.GetNames(typeof(ResourceType)).ToDictionary(s => s);
        }
    }
}
