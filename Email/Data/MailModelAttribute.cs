using System;

namespace CompositeC1Contrib.Email.Data
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MailModelAttribute : Attribute
    {
        public string Key { get; set; }
        public string TemplateName { get; set; }

        public MailModelAttribute(string key, string templateName)
        {
            Key = key;
            TemplateName = templateName;
        }
    }
}
