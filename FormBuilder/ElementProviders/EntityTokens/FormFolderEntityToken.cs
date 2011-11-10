using System;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.FormBuilder.ElementProviders.Tokens
{
    [SecurityAncestorProvider(typeof(FormFolderAncestorProvider))]
    public class FormFolderEntityToken : EntityToken
    {
        private string _id;
        public override string Id
        {
            get { return _id; }
        }

        public override string Serialize()
        {
            return base.DoSerialize();
        }

        public override string Source
        {
            get { return String.Empty; }
        }

        private string _type;
        public override string Type
        {
            get { return _type; }
        }

        public FormFolderEntityToken(Guid id, string type)
        {
            _id = id.ToString();
            _type = type;
        }

        public static EntityToken Deserialize(string serializedEntityToken)
        {
            string type;
            string source;
            string id;

            EntityToken.DoDeserialize(serializedEntityToken, out type, out source, out id);

            return new FormFolderEntityToken(Guid.Parse(id), type);
        }
    }
}
