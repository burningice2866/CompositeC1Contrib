using System;
using System.Net.Mime;

namespace CompositeC1Contrib.Email.Serialization
{
    [Serializable]
    public class SerializeableContentDisposition
    {
        DateTime CreationDate;
        String DispositionType;
        String FileName;
        Boolean Inline;
        DateTime ModificationDate;
        SerializeableCollection Parameters;
        DateTime ReadDate;
        long Size;

        public SerializeableContentDisposition(ContentDisposition contentDisposition)
        {
            CreationDate = contentDisposition.CreationDate;
            DispositionType = contentDisposition.DispositionType;
            FileName = contentDisposition.FileName;
            Inline = contentDisposition.Inline;
            ModificationDate = contentDisposition.ModificationDate;
            Parameters = new SerializeableCollection(contentDisposition.Parameters);
            ReadDate = contentDisposition.ReadDate;
            Size = contentDisposition.Size;
        }

        public void CopyTo(ContentDisposition contentDisposition)
        {
            contentDisposition.CreationDate = CreationDate;
            contentDisposition.DispositionType = DispositionType;
            contentDisposition.FileName = FileName;
            contentDisposition.Inline = Inline;
            contentDisposition.ModificationDate = ModificationDate;
            contentDisposition.ReadDate = ReadDate;
            contentDisposition.Size = Size;

            Parameters.CopyTo(contentDisposition.Parameters);
        }
    }
}
