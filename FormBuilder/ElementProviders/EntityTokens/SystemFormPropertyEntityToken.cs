using System;

using Composite.C1Console.Security;
using System.Reflection;

namespace CompositeC1Contrib.FormBuilder.ElementProviders.Tokens
{
    [SecurityAncestorProvider(typeof(SystemFormPropertyAncestorProvider))]
    public class SystemFormPropertyEntityToken : EntityToken
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

        private string _id;
        public override string Id
        {
            get { return _id; }
        }

        public SystemFormPropertyEntityToken(PropertyInfo prop)
        {
            _type = prop.DeclaringType.AssemblyQualifiedName;
            _id = prop.Name;
        }

        public SystemFormPropertyEntityToken(string type, string name)
        {
            _type = type;
            _id = name;
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

            return new SystemFormPropertyEntityToken(type, id);
        }
    }
}
