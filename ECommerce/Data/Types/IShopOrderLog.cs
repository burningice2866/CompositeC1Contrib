using System;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;

namespace CompositeC1Contrib.ECommerce.Data.Types
{
    [AutoUpdateble]
    [DataScope("public")]
    [Title("Order log")]
    [KeyPropertyName("Id")]
    [LabelPropertyName("Id")]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    [ImmutableTypeId("90a40ce3-3140-48e7-abf0-0f1f7f6c26e7")]
    public interface IShopOrderLog : IData
    {
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("5772545f-b75b-40d1-b573-184800aa10e0")]
        Guid Id { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 32)]
        [ImmutableFieldId("3317633e-f10f-47c8-890e-fde9b16fc025")]
        [ForeignKey(typeof(IShopOrder), "Id", AllowCascadeDeletes = true)]
        string ShopOrderId { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.DateTime)]
        [ImmutableFieldId("d4b1a26b-1189-40d2-8ee8-8e2493131479")]
        DateTime Timestamp { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 64)]
        [ImmutableFieldId("c9031d6d-6510-4640-ac36-6da4ceb39b09")]
        string Title { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.LargeString, IsNullable = true)]
        [ImmutableFieldId("97f18e29-f514-40fa-b2e7-15f9881a94da")]
        string Data { get; set; }
    }
}
