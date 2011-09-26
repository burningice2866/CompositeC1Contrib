using System;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace CompositeC1Contrib.Email.Serialization
{
    [Serializable]
    public class SerializeableAttachment
    {
        String ContentId;
        SerializeableContentDisposition ContentDisposition;
        SerializeableContentType ContentType;
        Stream ContentStream;
        System.Net.Mime.TransferEncoding TransferEncoding;
        String Name;
        Encoding NameEncoding;

        public SerializeableAttachment(Attachment attachment)
        {
            ContentId = attachment.ContentId;
            ContentDisposition = new SerializeableContentDisposition(attachment.ContentDisposition);
            ContentType = new SerializeableContentType(attachment.ContentType);
            Name = attachment.Name;
            TransferEncoding = attachment.TransferEncoding;
            NameEncoding = attachment.NameEncoding;

            if (attachment.ContentStream != null)
            {
                byte[] bytes = new byte[attachment.ContentStream.Length];
                attachment.ContentStream.Read(bytes, 0, bytes.Length);

                ContentStream = new MemoryStream(bytes);
            }
        }

        public Attachment GetAttachment()
        {
            var attachment = new Attachment(ContentStream, Name)
            {
                ContentId = ContentId,
                ContentType = ContentType.GetContentType(),
                Name = Name,
                TransferEncoding = TransferEncoding,
                NameEncoding = NameEncoding,
            };

            ContentDisposition.CopyTo(attachment.ContentDisposition);

            return attachment;
        }   
    }
}
