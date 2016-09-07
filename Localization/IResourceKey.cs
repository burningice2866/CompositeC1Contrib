using System;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;

namespace CompositeC1Contrib.Localization
{
    [AutoUpdateble]
    [DataScope(DataScopeIdentifier.PublicName)]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    [ImmutableTypeId("f43cc46a-91ec-48ec-a5fe-6039bba41d22")]
    [Title("Localization Resource Key")]
    [LabelPropertyName("Key")]
    [KeyPropertyName("Id")]
    public interface IResourceKey : IData
    {
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("0d6cb64c-51db-4fc8-9e93-edd84fe8a311")]
        Guid Id { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.String, 128)]
        [ImmutableFieldId("17e0d7d3-1f4c-42c8-9050-29c26413543d")]
        string Key { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.String, 128, IsNullable = true)]
        [ImmutableFieldId("030afa96-984c-46eb-ad2f-5255a0376caa")]
        string ResourceSet { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.String, 128)]
        [ImmutableFieldId("de0adf81-c222-4e16-ae3e-7cda0c8585ed")]
        string Type { get; set; }
    }
}
