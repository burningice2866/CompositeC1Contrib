using Composite.Data;

namespace CompositeC1Contrib.Teasers.Data.Types
{
    public interface ITeaserDesign : IData
    {
        [ImmutableFieldId("65c386f9-a494-4d23-a55b-09e5fee234bb")]
        [StoreFieldType(PhysicalStoreFieldType.String, 64, IsNullable = true)]
        string DesignName { get; set; }
    }
}
