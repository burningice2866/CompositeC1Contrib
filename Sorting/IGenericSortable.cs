using Composite.Data;

namespace CompositeC1Contrib.Sorting
{
    public interface IGenericSortable : IData
    {
        [StoreFieldType(PhysicalStoreFieldType.Integer)]
        [ImmutableFieldId("b90baf57-f085-41e8-af18-3d470a752f5f")]
        [DefaultFieldIntValue(0)]
        [TreeOrdering(999)]
        int LocalOrdering { get; set; }
    }
}
