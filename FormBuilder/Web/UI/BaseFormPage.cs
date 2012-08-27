using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

using Composite.Data;

using CompositeC1Contrib.FormBuilder.Attributes;
using CompositeC1Contrib.FormBuilder.Validation;
using CompositeC1Contrib.RazorFunctions;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public abstract class BaseFormPage<T> : CompositeC1WebPage where T : BaseForm
    {
        protected T _form = null;
        protected T Form
        {
            get
            {
                if (_form == null)
                {
                    var type = ResolveFormType();
                    var ctor = type.GetConstructor(new[] { typeof(NameValueCollection) });

                    _form = (T)ctor.Invoke(new[] { Request.Form });
                }

                return _form;
            }
        }

        protected string SubmitButtonLabel
        {
            get
            {
                var label = "Opret";

                var labelAttribute = ResolveFormType().GetCustomAttributes(typeof(SubmitButtonLabelAttribute), false).FirstOrDefault() as SubmitButtonLabelAttribute;
                if (labelAttribute != null)
                {
                    label = labelAttribute.Label;
                }

                return label;
            }
        }

        protected bool IsSuccess
        {
            get { return IsPost && !ValidationResult.Any(); }
        }

        protected IEnumerable<FormValidationRule> ValidationResult
        {
            get { return Form.Validate(); }
        }

        protected IHtmlString Errors()
        {
            return Form.Errors();
        }

        protected HtmlForm BeginForm()
        {
            string rawUrl = HttpContext.Current.Request.RawUrl;
            var type = ResolveFormType();

            WriteLiteral("<form id=\""+ type.Name +"\" method=\"post\" class=\"Standard Center\"");

            var formTagAttributes = type.GetCustomAttributes(typeof(FormTagAttributesAttribute), true).FirstOrDefault() as FormTagAttributesAttribute;
            if (formTagAttributes != null)
            {
                WriteLiteral(" "+ formTagAttributes.Attributes);
            }

            WriteLiteral(">");

            return new HtmlForm(this);
        }

        protected IHtmlString WriteAllFields()
        {
            var sb = new StringBuilder();

            var props = ResolveFormType().GetProperties();
            var orderedProps = new Dictionary<PropertyInfo, int>();

            foreach (var prop in props)
            {
                var fieldLabel = prop.GetCustomAttributes(typeof(FieldLabelAttribute), true).FirstOrDefault();
                if (fieldLabel != null)
                {
                    var order = prop.GetCustomAttributes(typeof(FieldPositionAttribute), true).FirstOrDefault() as FieldPositionAttribute;
                    
                    orderedProps.Add(prop, order != null ? order.Position : int.MaxValue);
                }
            }

            foreach (var prop in orderedProps
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key))
            {
                var textBeforeAttribute = prop.GetCustomAttributes(typeof(TextBeforeAttribute), true).FirstOrDefault() as TextBeforeAttribute;
                if (textBeforeAttribute != null)
                {
                    sb.Append(textBeforeAttribute.Text);
                }

                sb.Append(FormRenderer.FieldFor(Form, prop).ToString());
            }

            return new HtmlString(sb.ToString());
        }

        public override void ExecutePageHierarchy()
        {
            if (IsSuccess)
            {
                Form.Submit();
            }

            base.ExecutePageHierarchy();
        }

        protected abstract Type ResolveFormType();
    }
}
