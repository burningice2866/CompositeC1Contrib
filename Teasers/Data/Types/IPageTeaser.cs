using Composite.Data;

using CompositeC1Contrib.Sorting;

namespace CompositeC1Contrib.Teasers.Data.Types
{
    [LabelPropertyName("Name")]
    public interface IPageTeaser : IPageFolderData, IGenericSortable, ITeaser
	{
		[ImmutableFieldId("3A2CE19B-54C6-40B8-B7E5-B75B7F58A158")]
		[StoreFieldType(PhysicalStoreFieldType.String, 16)]
		[GroupByPriority(1)]
		string Position { get; set; }

		[ImmutableFieldId("2d334425-f5e7-425b-8965-55403ea7abe2")]
		[StoreFieldType(PhysicalStoreFieldType.String, 256)]
		string AdditionalHeader { get; set; }

		[ImmutableFieldId("4B77615B-AAC1-44DE-866B-0DABBDAe49D4")]
		[StoreFieldType(PhysicalStoreFieldType.Boolean)]
		bool ShowOnDescendants { get; set; }
	}
}
