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
        [StoreFieldType(PhysicalStoreFieldType.String, 255)]
        [ImmutableFieldId("15b786bf-b56c-48f6-aaf0-155becb38c13")]
        string FunctionName { get; set; }
    }
}
