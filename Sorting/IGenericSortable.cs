using Composite.Data;

namespace CompositeC1Contrib.Sorting
{
    [AutoUpdateble]
    [ImmutableTypeId("b34dabec-7241-407b-b623-172ed45c409f")]
    [KeyPropertyName("Id")]
    [DataScope(DataScopeIdentifier.PublicName)]
    public interface IGenericSortable : IData
    {
        [StoreFieldType(PhysicalStoreFieldType.Integer)]
        [ImmutableFieldId("b90baf57-f085-41e8-af18-3d470a752f5f")]
        [DefaultFieldIntValue(0)]
        int LocalOrdering { get; set; }
    }
}
