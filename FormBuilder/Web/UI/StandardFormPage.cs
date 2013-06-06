using Composite.AspNet.Razor;
using Composite.Core.Xml;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public sealed class StandardFormPage<T> : FormsPage<T> where T : BaseForm
    {
        private XhtmlDocument before;
        private XhtmlDocument success;

        public StandardFormPage(XhtmlDocument before, XhtmlDocument success)
        {
            this.before = before;
            this.success = success;
        }

        public override void Execute()
        {
            ExecutePageHierarchy();
        }

        public override void ExecutePageHierarchy()
        {
            if (IsSuccess)
            {
                Write(Html.C1().Markup(success));
            }
            else
            {
                Write(Html.C1().Markup(before));

                Write("<p>Felter med <span class=\"required\">*</span> skal udfyldes.</p>");

                using (BeginForm())
                {
                    Write(Form.WriteErrors());
                    Write(WriteAllFields());

                    Write("<div class=\"Buttons\"><input type=\"submit\" value=\"" + SubmitButtonLabel + "\" name=\"SubmitForm\" /></div>");
                }
            }

            base.ExecutePageHierarchy();
        }
    }
}
