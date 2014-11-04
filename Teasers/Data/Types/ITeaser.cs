using System;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;
using Composite.Data.Validation.Validators;

namespace CompositeC1Contrib.Teasers.Data.Types
{
    [AutoUpdateble]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    [DataScope(DataScopeIdentifier.PublicName)]
    [LabelPropertyName("Name")]
    public interface ITeaser : IData
    {
        [ImmutableFieldId("0835041c-d44c-47e4-a81f-d528f5f18f89")]
        [StoreFieldType(PhysicalStoreFieldType.String, 256)]
        [NotNullValidator]
        [StringSizeValidator(0, 256)]
        string Name { get; set; }

        [ImmutableFieldId("b0dffc66-5e2b-425d-8ab5-4bfba43e8c71")]
        [StoreFieldType(PhysicalStoreFieldType.DateTime, IsNullable = true)]
        DateTime? PublishDate { get; set; }

        [ImmutableFieldId("851a9629-152c-4b75-932a-25ec98ae39e5")]
        [StoreFieldType(PhysicalStoreFieldType.DateTime, IsNullable = true)]
        DateTime? UnpublishDate { get; set; }
    }
}
