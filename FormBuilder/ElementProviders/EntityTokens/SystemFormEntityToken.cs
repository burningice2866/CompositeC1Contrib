using System;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.FormBuilder.ElementProviders.Tokens
{
    [SecurityAncestorProvider(typeof(SystemFormAncestorProvider))]
    public class SystemFormEntityToken : EntityToken
    {
        private string _type;
        public override string Type
        {
            get { return _type; }
        }

        public override string Source
        {
            get { return String.Empty; }
        }

        public override string Id
        {
            get { return string.Empty; }
        }

        public SystemFormEntityToken(Type type)
        {
            _type = type.AssemblyQualifiedName;
        }

        public SystemFormEntityToken(string type)
        {
            _type = type;
        }

        public override string Serialize()
        {
            return base.DoSerialize();
        }

        public static EntityToken Deserialize(string serializedEntityToken)
        {
            string type;
            string source;
            string id;

            EntityToken.DoDeserialize(serializedEntityToken, out type, out source, out id);

            return new SystemFormEntityToken(type);
        }
    }
}
