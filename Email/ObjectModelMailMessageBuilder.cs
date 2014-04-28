using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Composite.Core.Types;
using Composite.Core.Xml;
using Composite.Data.DynamicTypes;
using Composite.Functions;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email
{
    public class ObjectModelMailMessageBuilder : MailMessageBuilder
    {
        private readonly object _model;

        public ObjectModelMailMessageBuilder(IMailTemplate template, object mailModel) : base(template)
        {
            _model = mailModel;
        }

        protected override IDictionary<string, object> GetDictionaryFromModel()
        {
            return _model.GetType().GetProperties().Where(d => d.CanRead).ToDictionary(p => p.Name, p => p.GetValue(_model, null));
        }

        protected override string ResolveHtml(string body)
        {
            return ResolveHtml(body, new FunctionContextContainer(), s => ResolveFields(s, _model));
        }

        private static string ResolveFields(string body, object mailModel)
        {
            var doc = XhtmlDocument.Parse(body);

            var type = mailModel.GetType();
            var objectValues = type.GetProperties().ToDictionary(p => p.Name, p => p.PropertyType == typeof(XElement) ? p.GetValue(mailModel, null) : (object)(p.GetValue(mailModel, null)).ToString());

            var fieldReferenceElements = doc.Descendants(Namespaces.DynamicData10 + "fieldreference");
            foreach (var el in fieldReferenceElements)
            {
                var attr = el.Attribute("typemanagername");
                var t = TypeManager.GetType(attr.Value);

                if (t != null && t == type)
                {
                    attr.Value = type.AssemblyQualifiedName;
                }
            }

            var references = DynamicTypeMarkupServices.GetFieldReferenceDefinitions(doc, type.AssemblyQualifiedName).ToList();
            foreach (var el in references)
            {
                object value = null;

                if (objectValues.ContainsKey(el.FieldName))
                {
                    value = objectValues[el.FieldName];
                }

                el.FieldReferenceElement.ReplaceWith(value);
            }

            return doc.ToString();
        }
    }
}
