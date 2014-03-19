using System;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;

namespace CompositeC1Contrib.Teasers.Data.Types
{
	[AutoUpdateble]
	[DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
	[DataScope(DataScopeIdentifier.PublicName)]
	[ImmutableTypeId("FA2C9317-DCB1-4FFC-B62C-E7B10BE34354")]
	[KeyPropertyName("Id")]
	[LabelPropertyName("Title")]
	[Title("Shared Teaser Group")]
	public interface ISharedTeaserGroup : IData
	{
		[ImmutableFieldId("2706E8BB-9BAA-46FA-9AC8-5B5CE7CE6A3E")]
		[StoreFieldType(PhysicalStoreFieldType.Guid)]
		[FunctionBasedNewInstanceDefaultFieldValue("<f:function name=\"Composite.Utils.Guid.NewGuid\" xmlns:f=\"http://www.composite.net/ns/function/1.0\" />")]
		Guid Id { get; set; }

		[NotNullValidator]
		[ImmutableFieldId("CD78B220-F84A-46E5-9BFE-94DD6A429021")]
		[StoreFieldType(PhysicalStoreFieldType.String, 128)]
		string Title { get; set; }
	}
}
