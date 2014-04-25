using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

using ICSharpCode.SharpZipLib.Zip;

using Composite;
using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email
{
    public class MailModelsFacade
    {
        public static MailMessage BuildEmailMessage(object mailModel)
        {
            using (var data = new DataConnection())
            {
                var modelType = mailModel.GetType();
                var templates = data.Get<IMailTemplate>().Where(t => !String.IsNullOrEmpty(t.ModelType) && Type.GetType(t.ModelType) == modelType).ToList();

                if (templates.Count == 0)
                {
                    throw new ArgumentException("There is zero templates using the supplied model");
                }

                if (templates.Count > 1)
                {
                    throw new ArgumentException("There is more than one template using the supplied model");
                }

                return BuildEmailMessage(templates.Single(), mailModel);
            }
        }

        public static MailMessage BuildEmailMessage(string key, object mailModel)
        {
            using (var data = new DataConnection())
            {
                var template = data.Get<IMailTemplate>().SingleOrDefault(t => String.Compare(t.Key, key, StringComparison.OrdinalIgnoreCase) == 0);

                Verify.IsNotNull(template, "Unknown template '{0}'", key);

                return BuildEmailMessage(template, mailModel);
            }
        }

        public static MailMessage BuildEmailMessage(IMailTemplate template, object mailModel)
        {
            var msg = new MailMessage()
            {
                Subject = TemplateRenderer.ResolveText(template.Subject, mailModel),
                Body = TemplateRenderer.ResolveHtml(template.Body, mailModel),
                IsBodyHtml = true,
            };

            AppendMailAddresses(msg.To, template.To, mailModel);
            AppendMailAddresses(msg.CC, template.Cc, mailModel);
            AppendMailAddresses(msg.Bcc, template.Bcc, mailModel);

            if (template.EncryptMessage)
            {
                var byteArray = Encoding.Default.GetBytes(msg.Body);

                var fileStreamProviders = new Dictionary<string, Func<Stream>>
				{
					{"content.html", () => new MemoryStream((byteArray))}
				};

                for (int i = 0; i < msg.Attachments.Count; i++)
                {
                    var file = msg.Attachments[i];

                    fileStreamProviders.Add("Attached file (" + (i + 1) + ") " + file.Name, () => file.ContentStream);
                }

                var encryptedAttachment = GetPasswordProtectedZip(fileStreamProviders, template.EncryptPassword);

                msg.Attachments.Clear();
                msg.Attachments.Add(encryptedAttachment);

                msg.Body = "Encrypted message attached";
                msg.IsBodyHtml = false;
            }

            return msg;
        }

        private static void AppendMailAddresses(MailAddressCollection collection, string s, object model)
        {
            if (!String.IsNullOrEmpty(s))
            {
                var split = s.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in split)
                {
                    var resolvedPart = TemplateRenderer.ResolveText(part, model);
                    var address = new MailAddress(resolvedPart);

                    collection.Add(address);
                }
            }
        }

        private static Attachment GetPasswordProtectedZip(Dictionary<string, Func<Stream>> fileStreamProviders, string password)
        {
            var memStream = new MemoryStream();

            var zipOutput = new ZipOutputStream(memStream)
            {
                Password = password
            };

            zipOutput.SetLevel(3);

            foreach (var fileStreamProvider in fileStreamProviders)
            {
                var filename = fileStreamProvider.Key;
                var streamProvider = fileStreamProvider.Value;

                var zipEntry = new ZipEntry(filename)
                {
                    DateTime = DateTime.Now
                };

                zipOutput.PutNextEntry(zipEntry);

                using (var streamReader = streamProvider())
                {
                    streamReader.CopyTo(zipOutput, 4096);
                }

                zipOutput.CloseEntry();
            }

            zipOutput.IsStreamOwner = false;
            zipOutput.Close();
            memStream.Position = 0;

            return new Attachment(memStream, "EncryptedMail.zip", "application/zip");
        }
    }
}
