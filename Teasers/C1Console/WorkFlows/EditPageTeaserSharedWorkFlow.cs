using System;
using System.Linq;

using Composite.Data;

using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.C1Console.WorkFlows
{
    public class EditPageTeaserSharedWorkFlow : BaseEditPageTeaserWorkFlow<IPageTeaserShared>
    {
        public EditPageTeaserSharedWorkFlow() : base("\\InstalledPackages\\CompositeC1Contrib.Teasers\\EditPageTeaserSharedWorkFlow.xml") { }

        protected override void SaveBindings()
        {
            var dataSourceId = DataSourceId.Deserialize(GetBinding<string>("SerializedDataSourceId"));
            var teaser = (ISharedTeaser)DataFacade.GetDataFromDataSourceId(dataSourceId);

            Teaser.SharedTeaserType = teaser.DataSourceId.InterfaceType.AssemblyQualifiedName;
            Teaser.SharedTeaserId = teaser.Id;
        }

        protected override void LoadBindings()
        {
            if (!String.IsNullOrEmpty(Teaser.SharedTeaserType))
            {
                var sharedTeaserType = Type.GetType(Teaser.SharedTeaserType);
                var sharedTeaser = DataFacade.GetData(sharedTeaserType).Cast<ISharedTeaser>().Single(t => t.Id == Teaser.SharedTeaserId);
                var serializedDataSourceId = sharedTeaser.DataSourceId.DataId == null ? String.Empty : sharedTeaser.DataSourceId.Serialize();

                Bindings.Add("SerializedDataSourceId", serializedDataSourceId);
            }
            else
            {
                Bindings.Add("SerializedDataSourceId", String.Empty);
            }
        }
    }
}
