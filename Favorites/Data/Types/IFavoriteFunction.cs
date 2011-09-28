using System;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;

namespace CompositeC1Contrib.Favorites.Data.Types
{
    [AutoUpdateble]
    [KeyPropertyName("Id")]
    [DataScope(DataScopeIdentifier.PublicName)]
    [ImmutableTypeId("dba3097a-ce10-438b-a4e9-d1a7610123c6")]
    [Title("Favorite function")]
    [LabelPropertyName("Name")]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    public interface IFavoriteFunction : IData
    {
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("ee3a9f91-9330-4f47-9663-c37f1b036db9")]
        [FunctionBasedNewInstanceDefaultFieldValue("<f:function name=\"Composite.Utils.Guid.NewGuid\" xmlns:f=\"http://www.composite.net/ns/function/1.0\" />")]
        Guid Id { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 255)]
        [ImmutableFieldId("6e1624b6-8b2a-45e6-839f-15552f5ca412")]
        string Name { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.LargeString)]
        [ImmutableFieldId("1572f4b8-e25c-4f12-83d8-30e1f1af2e95")]
        string SerializedEntityToken { get; set; }
    }
}
