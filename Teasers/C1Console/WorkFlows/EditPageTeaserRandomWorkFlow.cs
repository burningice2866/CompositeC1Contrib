using System;

using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.C1Console.WorkFlows
{
    public class EditPageTeaserRandomWorkFlow : BaseEditPageTeaserWorkFlow<IPageTeaserRandom>
    {
        public EditPageTeaserRandomWorkFlow() : base("\\InstalledPackages\\CompositeC1Contrib.Teasers\\EditPageTeaserRandomWorkFlow.xml") { }

        protected override void SaveBindings()
        {
            Teaser.TeaserGroup = GetBinding<Guid>("TeaserGroup");
        }

        protected override void LoadBindings()
        {
            Bindings.Add("TeaserGroup", Teaser.TeaserGroup);
        }
    }
}
