using System;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.FormBuilder.ElementProviders.Tokens
{
    [SecurityAncestorProvider(typeof(SourceFolderAncestorProvider))]
    public class SourceFolderEntityToken : EntityToken
    {
        public override string Type
        {
            get { return String.Empty; }
        }

        private string _source;
        public override string Source
        {
            get { return _source; }
        }

        public override string Id
        {
            get { return String.Empty; }
        }

        public SourceFolderEntityToken(string source)
        {
            _source = source;
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

            return new SourceFolderEntityToken(source);
        }
    }
}
