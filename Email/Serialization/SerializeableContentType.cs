using System;
using System.Net.Mime;

namespace CompositeC1Contrib.Email.Serialization
{
    [Serializable]
    internal class SerializeableContentType
    {
        String Boundary;
        String CharSet;
        String MediaType;
        String Name;
        SerializeableCollection Parameters;

        public SerializeableContentType(ContentType contentType)
        {
            Boundary = contentType.Boundary;
            CharSet = contentType.CharSet;
            MediaType = contentType.MediaType;
            Name = contentType.Name;
            Parameters = new SerializeableCollection(contentType.Parameters);
        }

        public ContentType GetContentType()
        {
            var sct = new ContentType()
            {
                Boundary = Boundary,
                CharSet = CharSet,
                MediaType = MediaType,
                Name = Name,
            };

            Parameters.CopyTo(sct.Parameters);

            return sct;
        }
    }
}
