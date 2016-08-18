using System;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;
using Composite.Data.Validation.Validators;

namespace CompositeC1Contrib.ECommerce.Data.Types
{
    [DataScope("public")]
    [Title("Order")]
    [LabelPropertyName("Id")]
    [AutoUpdateble]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    [ImmutableTypeId("1b157df8-a8be-4da2-bc00-442f0ff3126a")]
    [KeyPropertyName("Id")]
    public interface IShopOrder : IData
    {
        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 32)]
        [ImmutableFieldId("111229d0-11c0-4d8f-9fa9-d9749741f02a")]
        string Id { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.DateTime)]
        [ImmutableFieldId("c5cad8a9-71ed-4cf6-ac8e-28270741b4a8")]
        DateTime CreatedOn { get; set; }

        [DecimalPrecisionValidator(10, 2)]
        [StoreFieldType(PhysicalStoreFieldType.Decimal, 10, 2)]
        [ImmutableFieldId("a24298c5-9f03-4b2c-9038-3ae84bd1ac44")]
        decimal OrderTotal { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.String, 3, IsNullable = true)]
        [ImmutableFieldId("bf876a7f-26ae-48b5-a4fe-edcbcfc97c2d")]
        string Currency { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.LargeString, IsNullable = true)]
        [ImmutableFieldId("e8863c6c-afdd-4d24-b562-c93f2735ee11")]
        string AuthorizationXml { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.String, 32, IsNullable = true)]
        [ImmutableFieldId("e4f2bdd9-bf66-4572-92ae-ca51bef02f43")]
        string AuthorizationTransactionId { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.Integer)]
        [ImmutableFieldId("8edef5d0-4deb-415b-988e-ac6dc91abe45")]
        int PaymentStatus { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.String, 10, IsNullable = true)]
        [ImmutableFieldId("a0097cad-cf19-4592-851c-d54805cc1962")]
        string CreditCardType { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.Boolean)]
        [ImmutableFieldId("73279466-21f4-4795-ae73-4f193d9b28d8")]
        bool PostProcessed { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.LargeString, IsNullable = true)]
        [ImmutableFieldId("aa18e186-0364-4e50-a509-70930fcf23ec")]
        string CustomData { get; set; }
    }
}
