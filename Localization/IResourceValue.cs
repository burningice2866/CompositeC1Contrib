using System;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;

namespace CompositeC1Contrib.Localization
{
    [AutoUpdateble]
    [DataScope(DataScopeIdentifier.PublicName)]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    [ImmutableTypeId("12098982-afab-4884-b11d-ea9058cad016")]
    [Title("Localization Resource Value")]
    [LabelPropertyName("KeyId")]
    [KeyPropertyName("Id")]
    public interface IResourceValue : IData
    {
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("335eb415-764b-4f6b-a878-112669c4ce71")]
        Guid Id { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("e9963b59-b7a5-4d5e-928d-ccaee1d68678")]
        [ForeignKey(typeof(IResourceKey), "Id", AllowCascadeDeletes = true)]
        Guid KeyId { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.String, 16, IsNullable = false)]
        [ImmutableFieldId("f70ba0b5-ab10-4757-b2a4-adae7f13bfae")]
        [NotNullValidator]
        string Culture { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.LargeString)]
        [ImmutableFieldId("6829999d-70e2-4ede-a125-1be86caf8329")]
        string Value { get; set; }
    }
}
