using System;

using Composite.Data;

using CompositeC1Contrib.Teasers.C1Console.WorkFlows;

namespace CompositeC1Contrib.Teasers.Data.Types
{
	[ImmutableTypeId("DA18D853-D2DD-4C6D-AE41-7D3FD91CCE48")]
	[Title("Page Teaser (random)")]
    [Icon("refresh")]
    [EditWorkflow(typeof(EditPageTeaserRandomWorkFlow))]
	public interface IPageTeaserRandom : IPageTeaser
	{
		[ImmutableFieldId("8BDF67AF-DE2B-418F-84AF-BC65E1893515")]
		[StoreFieldType(PhysicalStoreFieldType.Guid)]
		[ForeignKey(typeof(ISharedTeaserGroup), "Id", AllowCascadeDeletes = true)]
		Guid TeaserGroup { get; set; }
	}
}
