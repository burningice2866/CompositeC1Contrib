using Composite.Core.Xml;

using CompositeC1Contrib.Teasers.Configuration;
using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.C1Console.WorkFlows
{
    public class EditPageTeaserFreeFormatWorkFlow : BaseEditPageTeaserWorkFlow<IPageTeaserFreeFormat>
    {
        public EditPageTeaserFreeFormatWorkFlow() : base("\\InstalledPackages\\CompositeC1Contrib.Teasers\\EditPageTeaserFreeFormatWorkFlow.xml") { }

        protected override void SaveBindings()
        {
            Teaser.Content = GetBinding<string>("Content") ?? new XhtmlDocument().ToString();
            Teaser.DesignName = GetBinding<string>("DesignName");
        }

        protected override void LoadBindings()
        {
            var designs = TeasersSection.GetSection().Designs;

            Bindings.Add("Content", Teaser.Content);
            Bindings.Add("Designs", designs.Count > 0 ? designs : null);
            Bindings.Add("DesignName", Teaser.DesignName);
        }
    }
}
