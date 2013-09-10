using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using Composite.AspNet.Razor;
using Composite.Core.Xml;
using Composite.Functions;

using CompositeC1Contrib.FormBuilder.Attributes;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public abstract class FormsPage : RazorFunction
    {
        protected FormOptions Options { get; private set; }

        [FunctionParameter(Label = "Intro text", DefaultValue = null)]
        public XhtmlDocument IntroText { get; set; }

        [FunctionParameter(Label = "Success response", DefaultValue = null)]
        public XhtmlDocument SuccessResponse { get; set; }

        public FormsPage()
        {
            Options = new FormOptions();
        }

        protected bool IsOwnSubmit
        {
            get { return IsPost && Request.Form["__type"] == FormModel.Current.Name; }
        }

        protected bool IsSuccess
        {
            get { return IsOwnSubmit && !FormModel.Current.ValidationResult.Any(); }
        }

        protected string SubmitButtonLabel
        {
            get
            {
                var label = "Opret";

                var labelAttribute = FormModel.Current.Attributes.OfType<SubmitButtonLabelAttribute>().FirstOrDefault();
                if (labelAttribute != null)
                {
                    label = labelAttribute.Label;
                }

                return label;
            }
        }

        public override void ExecutePageHierarchy()
        {
            var model = FormModel.Current;

            if (model.ForceHttps && !Request.IsSecureConnection)
            {
                string redirectUrl = Request.Url.ToString().Replace("http:", "https:");

                Response.Redirect(redirectUrl, true);
            }

            if (IsOwnSubmit)
            {
                var requestFiles = Request.Files;
                var files = new List<FormFile>();

                for (int i = 0; i < requestFiles.Count; i++)
                {
                    var f = requestFiles[i];

                    files.Add(new FormFile()
                    {
                        Key = requestFiles.AllKeys[i],
                        ContentLength = f.ContentLength,
                        ContentType = f.ContentType,
                        FileName = f.FileName,
                        InputStream = f.InputStream
                    });
                }

                model.MapValues(Request.Form, files);

                OnMappedValues();

                model.Validate();

                if (IsSuccess)
                {
                    OnSubmit();

                    model.OnSubmitHandler();
                }
            }

            base.ExecutePageHierarchy();
        }

        protected virtual void OnMappedValues() { }
        protected virtual void OnSubmit() { }

        protected IHtmlString WriteErrors()
        {
            if (IsOwnSubmit)
            {
                return FormRenderer.WriteErrors(FormModel.Current);
            }
            else
            {
                return new HtmlString(String.Empty);
            }
        }

        protected IHtmlString WriteAllFields()
        {
            var sb = new StringBuilder();

            foreach (var field in FormModel.Current.Fields.Where(f => f.Label != null))
            {
                sb.Append(FormRenderer.FieldFor(field).ToString());
            }

            return new HtmlString(sb.ToString());
        }

        protected bool HasDependencyChecks()
        {
            return FormModel.Current.Fields.Select(f => f.DependencyAttributes).Any();
        }

        protected HtmlForm BeginForm()
        {
            return BeginForm(null);
        }

        protected HtmlForm BeginForm(object htmlAttributes)
        {
            return new HtmlForm(this, FormModel.Current, htmlAttributes);
        }

        protected string WriteErrorClass(string name)
        {
            var validationResult = FormModel.Current.ValidationResult;

            return FormRenderer.WriteErrorClass(name, validationResult);
        }
    }
}
