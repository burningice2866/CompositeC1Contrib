using System.Collections.Generic;

using CompositeC1Contrib.Rendering.Nancy.Functions;

namespace CompositeC1Contrib.Rendering.Nancy
{
    public class GlobalConfiguration
    {
        public static GlobalConfiguration Current = new GlobalConfiguration();

        public IList<NancyFunction> Functions { get; private set; }

        public GlobalConfiguration()
        {
            Functions = new List<NancyFunction>();
        }
    }
}
