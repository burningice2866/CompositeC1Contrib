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
        protected FormModel RenderingModel { get; set; }
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
            get { return IsPost && Request.Form["__type"] == RenderingModel.Name; }
        }

        protected bool IsSuccess
        {
            get { return IsOwnSubmit && !RenderingModel.ValidationResult.Any(); }
        }

        protected string SubmitButtonLabel
        {
            get
            {
                var label = "Opret";

                var labelAttribute = RenderingModel.Attributes.OfType<SubmitButtonLabelAttribute>().FirstOrDefault();
                if (labelAttribute != null)
                {
                    label = labelAttribute.Label;
                }

                return label;
            }
        }

        public override void ExecutePageHierarchy()
        {
            if (RenderingModel.ForceHttps && !Request.IsSecureConnection)
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

                RenderingModel.MapValuesAndValidate(Request.Form, files);

                if (IsSuccess)
                {
                    OnSubmit();

                    RenderingModel.OnSubmitHandler();
                }
            }

            base.ExecutePageHierarchy();
        }

        protected virtual void OnSubmit() { }

        protected IHtmlString WriteErrors()
        {
            if (IsOwnSubmit)
            {
                return FormRenderer.WriteErrors(RenderingModel);
            }
            else
            {
                return new HtmlString(String.Empty);
            }
        }

        protected IHtmlString WriteAllFields()
        {
            var sb = new StringBuilder();

            foreach (var field in RenderingModel.Fields)
            {
                sb.Append(FormRenderer.FieldFor(field).ToString());
            }

            return new HtmlString(sb.ToString());
        }

        protected bool HasDependencyChecks()
        {
            return RenderingModel.Fields.Select(f => f.DependencyAttributes).Any();
        }

        protected HtmlForm BeginForm()
        {
            return BeginForm(null);
        }

        protected HtmlForm BeginForm(object htmlAttributes)
        {
            var @class = "form formbuilder-" + RenderingModel.Name.ToLowerInvariant();
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

            WriteLiteral(String.Format("<form method=\"post\" class=\"{1}\" action=\"{2}\"", RenderingModel.Name, @class, action));

            if (RenderingModel.HasFileUpload)
            {
                WriteLiteral(" enctype=\"multipart/form-data\"");
            }

            var formTagAttributes = RenderingModel.Attributes.OfType<FormTagAttributesAttribute>().FirstOrDefault();
            if (formTagAttributes != null)
            {
                WriteLiteral(" " + formTagAttributes.Attributes);
            }

            WriteLiteral(">");

            WriteLiteral("<input type=\"hidden\" name=\"__type\" value=\"" + RenderingModel.Name + "\" />");

            return new HtmlForm(this);
        }

        protected string WriteErrorClass(string name)
        {
            var validationResult = RenderingModel.ValidationResult;

            return FormRenderer.WriteErrorClass(name, validationResult);
        }
    }
}
