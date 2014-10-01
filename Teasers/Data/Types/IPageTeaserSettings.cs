using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;

namespace CompositeC1Contrib.Teasers.Data.Types
{
	[AutoUpdateble]
	[DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
	[DataScope(DataScopeIdentifier.PublicName)]
	[ImmutableTypeId("45cf0a61-c4f9-b01a-41f5-65c9621b55d9")]
    [Title("Teaser Settings")]
	public interface IPageTeaserSettings : IPageMetaData
	{
		[ImmutableFieldId("7f616aeb-4df0-4c2e-82b8-4c1c2aa705b6")]
		[StoreFieldType(PhysicalStoreFieldType.Boolean)]
		bool HideAncestorTeasers { get; set; }
	}
}
