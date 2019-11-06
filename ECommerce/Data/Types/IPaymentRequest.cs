using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace CompositeC1Contrib.ECommerce.Data.Types
{
    [AutoUpdateble]
    [DataScope("public")]
    [Title("Payment request")]
    [KeyPropertyName("ShopOrderId")]
    [LabelPropertyName("ShopOrderId")]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    [ImmutableTypeId("9d91e5b0-96b9-4dec-bf47-a79fb6bec80e")]
    public interface IPaymentRequest : IData
    {
        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 32)]
        [ImmutableFieldId("fbf4c965-addd-4fbd-a46d-2ff446320fda")]
        [ForeignKey(typeof(IShopOrder), "Id", AllowCascadeDeletes = true)]
        string ShopOrderId { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 256)]
        [ImmutableFieldId("2c08f68a-352e-4329-999b-bf2ef3d60863")]
        string ProviderName { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.Boolean)]
        [ImmutableFieldId("0bf59b84-c49f-4331-baa7-a359f17daf15")]
        bool Accepted { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.LargeString, IsNullable = true)]
        [ImmutableFieldId("cca52f3d-aeb1-4145-a32f-5fae9a65287a")]
        string AuthorizationData { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.String, 32, IsNullable = true)]
        [ImmutableFieldId("47daf0cc-c154-45b7-8ace-f52804e91b15")]
        string AuthorizationTransactionId { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.String, 16, IsNullable = true)]
        [ImmutableFieldId("91b15b0e-77ff-41ad-a984-a74b72d67879")]
        string PaymentMethod { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.LargeString, IsNullable = true)]
        [ImmutableFieldId("995019d6-90a8-4451-97fb-6372cb664668")]
        string CancelUrl { get; set; }
    }
}
