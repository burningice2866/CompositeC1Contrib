using System;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

using Composite.C1Console.Security;
using Composite.C1Console.Workflow;
using Composite.Core.WebClient;
using Composite.Data;

using CompositeC1Contrib.Teasers.C1Console.EntityTokens;
using CompositeC1Contrib.Teasers.C1Console.WorkFlows;
using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.Web.UI
{
    public static class TeaserHtmlHelper
    {
        public static IHtmlString RenderScriptTags(this HtmlHelper helper)
        {
            if (DataScopeManager.CurrentDataScope != DataScopeIdentifier.Administrated)
            {
                return helper.Raw(String.Empty);
            }

            var s = ScriptLoader.Render("sub") + "<script src=\"/Composite/InstalledPackages/CompositeC1Contrib.Teasers/teaserConsoleFunctions.js\"></script>";

            return helper.Raw(s);
        }

        public static IHtmlString RenderFocusLink(this HtmlHelper helper, IPageTeaser teaser, string label)
        {
            if (DataScopeManager.CurrentDataScope != DataScopeIdentifier.Administrated)
            {
                return helper.Raw(String.Empty);
            }

            var page = PageManager.GetPageById(teaser.PageId);
            var token = new PageTeaserInstanceEntityToken(page, teaser);
            var serializedToken = EntityTokenSerializer.Serialize(token, true);

            return helper.Raw("<a href=\"#\" data-token=\"" + serializedToken + "\" onclick=\"setFocus(this)\">" + label + "</a>");
        }

        public static IHtmlString RenderEditLink(this HtmlHelper helper, IPageTeaser teaser, string label)
        {
            if (DataScopeManager.CurrentDataScope != DataScopeIdentifier.Administrated)
            {
                return helper.Raw(String.Empty);
            }

            var editWorkflowAttribute = teaser.DataSourceId.InterfaceType.GetCustomAttributes(true).OfType<EditWorkflowAttribute>().FirstOrDefault();
            if (editWorkflowAttribute == null)
            {
                return helper.Raw(String.Empty);
            }

            var page = PageManager.GetPageById(teaser.PageId);
            var entityToken = new PageTeaserInstanceEntityToken(page, teaser);
            var serializedEntityToken = EntityTokenSerializer.Serialize(entityToken, true);

            var editActionToken = new WorkflowActionToken(editWorkflowAttribute.EditWorkflowType);
            var serializedActionToken = ActionTokenSerializer.Serialize(editActionToken, true);

            var html = String.Format("<a href=\"#\" data-providername=\"{0}\" data-entitytoken=\"{1}\" data-actiontoken=\"{2}\" data-piggybag=\"{3}\" data-piggybaghash=\"{4}\" onclick=\"executeAction(this)\">{5}</a>",
                teaser.DataSourceId.ProviderName,
                serializedEntityToken,
                serializedActionToken,
                String.Empty,
                HashSigner.GetSignedHash(string.Empty).Serialize(),
                label);

            return helper.Raw(html);
        }
    }
}
