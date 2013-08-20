using System;
using System.Collections.Generic;
using System.Linq;

using CompositeC1Contrib.FormBuilder.Attributes;
using CompositeC1Contrib.FormBuilder.Validation;
using CompositeC1Contrib.FormBuilder.Web.UI;

namespace CompositeC1Contrib.FormBuilder
{
    public class FormField
    {
        public FormModel OwningForm { get; private set; }

        public string Name { get; set; }
        public IEnumerable<Attribute> Attributes { get; private set; }
        public object Value { get; set; }
        public Type ValueType { get; private set; }

        public string Id
        {
            get { return (OwningForm.Name + Name).Replace(".", "_"); }
        }

        public FieldLabelAttribute Label
        {
            get { return Attributes.OfType<FieldLabelAttribute>().First(); }
        }

        public string PlaceholderText
        {
            get
            {
                var placeholderAttr = Attributes.OfType<PlaceholderTextAttribute>().SingleOrDefault();
                if (placeholderAttr == null)
                {
                    return Label.Label;
                }

                return placeholderAttr.Text;
            }
        }

        public string Help
        {
            get
            {
                var helpAttribute = Attributes.OfType<FieldHelpAttribute>().FirstOrDefault();
                if (helpAttribute != null)
                {
                    return helpAttribute.Help;
                }

                return String.Empty;
            }
        }

        public IInputElementHandler InputTypeHandler
        {
            get
            {
                var inputTypeAttribute = Attributes.OfType<InputElementProviderAttribute>().FirstOrDefault();
                if (inputTypeAttribute != null)
                {
                    return inputTypeAttribute.GetInputFieldTypeHandler();
                }

                return GetDefaultInputType(ValueType);
            }
        }

        public bool IsRequired
        {
            get { return Attributes.Any(a => a is RequiredFieldAttribute); }
        }

        public IEnumerable<RichTextListItem> DataSource
        {
            get
            {
                var datasourceAttribute = Attributes.OfType<DataSourceAttribute>().FirstOrDefault();
                if (datasourceAttribute == null)
                {
                    return null;
                }

                var ds = datasourceAttribute.GetData();
                if (ds == null)
                {
                    return null;
                }

                var dict = ds as IDictionary<string, string>;
                if (dict != null)
                {
                    return dict.Select(f => new RichTextListItem(f.Key, f.Value));
                }

                if (ds is IEnumerable<string>)
                {
                    return (ds as IEnumerable<string>).Select(str => new RichTextListItem(str, str));
                }

                var list = ds as IEnumerable<RichTextListItem>;
                if (list != null)
                {
                    return list;
                }

                throw new InvalidOperationException("Unsupported data source type: " + ds.GetType().FullName);
            }
        }

        public IEnumerable<FormValidationAttribute> ValidationAttributes
        {
            get { return Attributes.OfType<FormValidationAttribute>(); }
        }

        public IEnumerable<FormDependencyAttribute> DependencyAttributes
        {
            get { return Attributes.OfType<FormDependencyAttribute>(); }
        }

        public FormField(FormModel owningForm, string name, Type valueType, IList<Attribute> attributes)
        {
            OwningForm = owningForm;
            Name = name;
            Attributes = attributes;
            ValueType = valueType;
        }

        private static IInputElementHandler GetDefaultInputType(Type type)
        {
            if (type == typeof(bool)) return new CheckboxInputElement();
            if (type == typeof(FormFile) || type == typeof(IEnumerable<FormFile>)) new FileuploadInputElement();

            return new TextboxInputElement();
        }
    }
}
