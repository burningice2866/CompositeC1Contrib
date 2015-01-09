using System;

using Composite.C1Console.Security;
using Composite.Functions;

namespace CompositeC1Contrib.Rendering.Nancy.Functions
{
    [SecurityAncestorProvider(typeof(StandardFunctionSecurityAncestorProvider))]
    public class NancyFunctionEntityToken : EntityToken
    {
        private readonly string _id;
        public override string Id
        {
            get { return _id; }
        }

        public override string Source
        {
            get { return String.Empty; }
        }

        public override string Type
        {
            get { return String.Empty; }
        }

        public NancyFunctionEntityToken(NancyFunction function)
        {
            _id = function.Namespace + "." + function.Name;
        }

        public NancyFunctionEntityToken(string fullName)
        {
            _id = fullName;
        }

        public override string Serialize()
        {
            return DoSerialize();
        }

        public static EntityToken Deserialize(string serializedData)
        {
            string type;
            string source;
            string id;

            DoDeserialize(serializedData, out type, out source, out id);

            return new NancyFunctionEntityToken(id);
        }
    }
}
