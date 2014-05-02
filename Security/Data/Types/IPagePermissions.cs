using System;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;
using Composite.Data.Types;

namespace CompositeC1Contrib.Security.Data.Types
{
    [AutoUpdateble]
    [KeyPropertyName("Id")]
    [DataScope(DataScopeIdentifier.PublicName)]
    [ImmutableTypeId("d9fc9f5a-1d23-42ea-8b62-a52df6f50ffd")]
    [Title("Page permission")]
    [LabelPropertyName("PageId")]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    public interface IPagePermissions : IData
    {
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("6F9F9392-C91C-42C7-B0B3-D44B2AFC50C5")]
        [FunctionBasedNewInstanceDefaultFieldValue("<f:function name=\"Composite.Utils.Guid.NewGuid\" xmlns:f=\"http://www.composite.net/ns/function/1.0\" />")]
        Guid Id { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("C8B916ED-88F9-43C0-8B26-3DFCA1EA6CD9")]
        [ForeignKey(typeof(IPage), "Id", AllowCascadeDeletes = true)]
        Guid PageId { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.LargeString)]
        [ImmutableFieldId("1E1B4EB7-A41C-4E0B-BD8F-8005A9633BFB")]
        string AllowedRoles { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.LargeString)]
        [ImmutableFieldId("0C324BF5-918C-4F3B-A390-B570DEA0B86A")]
        string DeniedRoles { get; set; }
    }
}
