using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;

using Composite.AspNet.Razor;
using Composite.Data;

using CompositeC1Contrib.FormBuilder.Attributes;
using CompositeC1Contrib.FormBuilder.Dependencies;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public abstract class FormsPage<T> : RazorFunction where T : BaseForm
    {
        protected FormOptions Options { get; set; }

        protected T _form = null;
        protected T Form
        {
            get
            {
                if (_form == null)
                {
                    var type = typeof(T);
                    var ctor = type.GetConstructor(new[] { typeof(NameValueCollection), typeof(IEnumerable<FormFile>) });

                    if (IsOwnSubmit)
                    {
                        var requestFiles = Request.Files;
                        var files = requestFiles.AllKeys.Select(k => requestFiles[k]).Select(f => new FormFile()
                        {
                            ContentLength = f.ContentLength,
                            ContentType = f.ContentType,
                            FileName = f.FileName,
                            InputStream = f.InputStream
                        });

                        _form = (T)ctor.Invoke(new object[] { Request.Form, files });
                    }
                    else
                    {
                        _form = (T)ctor.Invoke((new object[] { null, null }));
                    }
                }

                return _form;
            }
        }

        protected string SubmitButtonLabel
        {
            get
            {
                var label = "Opret";

                var labelAttribute = typeof(T).GetCustomAttributes(typeof(SubmitButtonLabelAttribute), false).FirstOrDefault() as SubmitButtonLabelAttribute;
                if (labelAttribute != null)
                {
                    label = labelAttribute.Label;
                }

                return label;
            }
        }

        protected bool IsOwnSubmit
        {
            get { return IsPost && Request.Form["__type"] == typeof(T).FullName; }
        }

        protected bool IsSuccess
        {
            get { return IsOwnSubmit && !Form.Validate().Any(); }
        }

        public FormsPage()
        {
            Options = new FormOptions();
        }

        protected IHtmlString WriteErrors()
        {
            if (IsOwnSubmit)
            {
                return Form.WriteErrors();
            }
            else
            {
                return new HtmlString(String.Empty);
            }
        }

        protected HtmlForm BeginForm()
        {
            return BeginForm(null);
        }

        protected HtmlForm BeginForm(object htmlAttributes)
        {
            var type = typeof(T);
            var @class = "form formbuilder-" + type.Name.ToLowerInvariant();
            var action = String.Empty;

            var dictionary = Functions.ObjectToDictionary(htmlAttributes);
            if (dictionary != null)
            {
                if (dictionary.ContainsKey("class"))
                {
                    @class += " " + (string)dictionary["class"];
                }

                if (dictionary.ContainsKey("action"))
                {
                    action = (string)dictionary["action"];
                }
            }

            WriteLiteral(String.Format("<form method=\"post\" class=\"{1}\" action=\"{2}\"", type.Name, @class, action));

            if (HasFileUpload())
            {
                WriteLiteral(" enctype=\"multipart/form-data\"");
            }

            var formTagAttributes = type.GetCustomAttributes(typeof(FormTagAttributesAttribute), true).FirstOrDefault() as FormTagAttributesAttribute;
            if (formTagAttributes != null)
            {
                WriteLiteral(" " + formTagAttributes.Attributes);
            }

            WriteLiteral(">");

            WriteLiteral("<input type=\"hidden\" name=\"__type\" value=\"" + Form.GetType().FullName + "\" />");

            return new HtmlForm(this);
        }

        protected bool HasDependencyChecks()
        {
            return typeof(T).GetProperties().Select(f => f.GetCustomAttributes(true).OfType<FormDependencyAttribute>()).Any();
        }

        protected bool HasFileUpload()
        {
            return typeof(T).GetProperties().Any(p => p.PropertyType == typeof(FormFile) || p.PropertyType == typeof(IEnumerable<FormFile>));
        }

        protected IHtmlString WriteAllFields()
        {
            var sb = new StringBuilder();

            var props = typeof(T).GetProperties();
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

                sb.Append(FormRenderer.FieldFor(Form, Options, prop).ToString());
            }

            return new HtmlString(sb.ToString());
        }

        public override void ExecutePageHierarchy()
        {
            if (IsPost)
            {
                var type = typeof(T);
                var submittedType = Request.Form["__type"] ?? String.Empty;

                if (type.FullName == submittedType && IsSuccess)
                {
                    Form.Submit();
                }
            }

            base.ExecutePageHierarchy();
        }

        protected IHtmlString FieldFor(Expression<Func<T, object>> field)
        {
            var htmlAttributes = new Dictionary<string, object>();

            return FieldFor(field, htmlAttributes);
        }

        protected IHtmlString FieldFor(Expression<Func<T, object>> field, object htmlAttributes)
        {
            var prop = Form.GetProperty(field);
            var dictionary = Functions.ObjectToDictionary(htmlAttributes);

            return FormRenderer.FieldFor(Form, Options, prop, dictionary);
        }

        protected IHtmlString NameFor(Expression<Func<T, object>> field)
        {
            var prop = Form.GetProperty(field);

            return FormRenderer.NameFor(Form, prop);
        }

        protected string WriteErrorClass(string name)
        {
            var validationResult = Form.Validate();

            return FormRenderer.WriteErrorClass(name, validationResult);
        }
    }
}
