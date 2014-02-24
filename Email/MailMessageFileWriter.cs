using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Hosting;

using CompositeC1Contrib.Email.Serialization;

namespace CompositeC1Contrib.Email
{
    public class MailMessageFileWriter
    {
        private static readonly string _path = HostingEnvironment.MapPath("~/App_Data/MailMessage/");

        static MailMessageFileWriter ()
        {
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
        }

        public static void SaveMailMessageToDisk(Guid id, MailMessage mailMessage)
        {
            var serializedMailMessage = new SerializeableMailMessage(mailMessage);

            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, serializedMailMessage);
                
                using (var fs = File.Create(Path.Combine(_path, id + ".bin")))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fs);
                }
            }

            var eml = ToEml(mailMessage);
            File.WriteAllText(Path.Combine(_path, id + ".eml"), eml);
        }

        private static string ToEml(MailMessage message)
        {
            var assembly = typeof(SmtpClient).Assembly;
            var mailWriterType = assembly.GetType("System.Net.Mail.MailWriter");

            using (var memoryStream = new MemoryStream())
            {
                // Get reflection info for MailWriter contructor
                var mailWriterContructor = mailWriterType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Stream) }, null);

                // Construct MailWriter object with our FileStream
                var mailWriter = mailWriterContructor.Invoke(new object[] { memoryStream });

                // Get reflection info for Send() method on MailMessage
                var sendMethod = typeof(MailMessage).GetMethod("Send", BindingFlags.Instance | BindingFlags.NonPublic);

                // Call method passing in MailWriter
                sendMethod.Invoke(message, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { mailWriter, true, true }, null);

                // Finally get reflection info for Close() method on our MailWriter
                var closeMethod = mailWriter.GetType().GetMethod("Close", BindingFlags.Instance | BindingFlags.NonPublic);

                // Call close method
                closeMethod.Invoke(mailWriter, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { }, null);

                return Encoding.ASCII.GetString(memoryStream.ToArray());
            }
        }
    }
}
