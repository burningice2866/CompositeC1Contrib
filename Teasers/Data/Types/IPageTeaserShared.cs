using System;

using Composite.Data;

using CompositeC1Contrib.Teasers.C1Console.WorkFlows;

namespace CompositeC1Contrib.Teasers.Data.Types
{
	[ImmutableTypeId("B685651B-A2E9-47B8-973A-5B61CB9E4A25")]
    [Title("${CompositeC1Contrib.Teasers, IPageTeaserShared.Title}")]
    [Icon("browser")]
    [EditWorkflow(typeof(EditPageTeaserSharedWorkFlow))]
	public interface IPageTeaserShared : IPageTeaser
	{
        [StoreFieldType(PhysicalStoreFieldType.String, 512)]
        [ImmutableFieldId("60e66751-1c65-4f6b-aa1f-35b4249f18cf")]
        string SharedTeaserType { get; set; }

		[StoreFieldType(PhysicalStoreFieldType.Guid)]
		[ImmutableFieldId("F12FA3EF-6FBF-488B-A8E3-1AB5B0F58535")]
		Guid SharedTeaserId { get; set; }
	}
}
