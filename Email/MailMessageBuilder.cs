using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Xml.Linq;

using Composite.Core.WebClient;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.Xml;
using Composite.Functions;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email
{
    public abstract class MailMessageBuilder
    {
        private readonly IMailTemplate _template;
        private readonly IList<Attachment> _attachments;

        protected MailMessageBuilder(IMailTemplate template)
        {
            _template = template;
            _attachments = new List<Attachment>();
        }

        public void AddAttachment(Attachment attachment)
        {
            _attachments.Add(attachment);
        }

        public MailMessage BuildMailMessage()
        {
            var mailMessage = new MailMessage
            {
                Subject = ResolveText(_template.Subject),
                Body = ResolveHtml(_template.Body),
                IsBodyHtml = true
            };

            if (!String.IsNullOrEmpty(_template.From))
            {
                var resolvedFrom = ResolveText(_template.From);

                mailMessage.From = new MailAddress(resolvedFrom);
            }

            AppendMailAddresses(mailMessage.To, _template.To);
            AppendMailAddresses(mailMessage.CC, _template.Cc);
            AppendMailAddresses(mailMessage.Bcc, _template.Bcc);

            mailMessage.Headers.Add("X-C1Contrib-Mail-TemplateKey", _template.Key);

            foreach (var attachment in _attachments)
            {
                mailMessage.Attachments.Add(attachment);
            }

            if (_template.EncryptMessage)
            {
                MailsFacade.EncryptMessage(mailMessage, _template.EncryptPassword);
            }

            _attachments.Clear();

            return mailMessage;
        }

        protected string ResolveText(string text)
        {
            var dict = GetDictionaryFromModel();

            return dict.Aggregate(text, (current, kvp) => ReplaceText(current, kvp.Key, kvp.Value));
        }

        protected string ResolveHtml(string body, FunctionContextContainer functionContextContainer, Func<string, string> resolveHtmlFunction)
        {
            var dict = GetDictionaryFromModel();
            var doc = XhtmlDocument.Parse(body);

            PageRenderer.ExecuteEmbeddedFunctions(doc.Root, functionContextContainer);

            body = doc.ToString();
            body = PageUrlHelper.ChangeRenderingPageUrlsToPublic(body);
            body = resolveHtmlFunction(body);

            doc = XhtmlDocument.Parse(body);

            AppendHostnameToAbsolutePaths(doc);
            ResolveTextInLinks(doc, dict);

            return doc.ToString();
        }

        protected abstract IDictionary<string, object> GetDictionaryFromModel();
        protected abstract string ResolveHtml(string body);

        private static void ResolveTextInLinks(XhtmlDocument doc, IDictionary<string, object> model)
        {
            foreach (var kvp in model)
            {
                var elements = doc.Descendants().Where(el => el.Name.LocalName == "a").ToList();
                foreach (var a in elements)
                {
                    var href = a.Attribute("href");
                    if (href == null)
                    {
                        continue;
                    }

                    href.Value = ReplaceText(href.Value, kvp.Key, kvp.Value);
                }
            }
        }

        private static void AppendHostnameToAbsolutePaths(XhtmlDocument doc)
        {
            var ctx = HttpContext.Current;
            if (ctx == null)
            {
                return;
            }

            var elements = doc.Descendants().Where(f => f.Name.Namespace == Namespaces.Xhtml);
            var pathAttributes = elements.Attributes().Where(f => f.Name.LocalName == "src" || f.Name.LocalName == "href" || f.Name.LocalName == "action");

            var absolutePathAttributes = pathAttributes.Where(f => f.Value.StartsWith("/")).ToList();
            if (!absolutePathAttributes.Any())
            {
                return;
            }

            var hostName = ctx.Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped);
            var baseUri = new Uri(hostName);

            foreach (var attr in absolutePathAttributes)
            {
                attr.Value = new Uri(baseUri, attr.Value).OriginalString;
            }
        }

        private void AppendMailAddresses(MailAddressCollection collection, string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return;
            }

            var split = s.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in split)
            {
                var resolvedPart = ResolveText(part);
                var address = new MailAddress(resolvedPart);

                collection.Add(address);
            }
        }

        private static string ReplaceText(string text, string name, object value)
        {
            string s = (value ?? String.Empty).ToString();

            return text.Replace(String.Format("%{0}%", name), s);
        }
    }
}
