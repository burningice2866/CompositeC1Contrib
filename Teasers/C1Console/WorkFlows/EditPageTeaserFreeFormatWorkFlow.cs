using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.C1Console.WorkFlows
{
    public class EditPageTeaserFreeFormatWorkFlow : BaseEditPageTeaserWorkFlow<IPageTeaserFreeFormat>
    {
        public EditPageTeaserFreeFormatWorkFlow() : base("\\InstalledPackages\\CompositeC1Contrib.Teasers\\EditPageTeaserFreeFormatWorkFlow.xml") { }

        protected override void SaveBindings()
        {
            Teaser.Content = GetBinding<string>("Content");
        }

        protected override void LoadBindings()
        {
            Bindings.Add("Content", Teaser.Content);
        }
    }
}
