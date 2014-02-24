using System;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;

namespace CompositeC1Contrib.Email.Data.Types
{
    [AutoUpdateble]
    [KeyPropertyName("Id")]
    [LabelPropertyName("Key")]
    [Title("Mail template")]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    [ImmutableTypeId("b1861a88-4f03-4386-be5c-cd8f976498e7")]
    [DataScope(DataScopeIdentifier.PublicName)]
    public interface IMailTemplate : IData
    {
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("ba04e85a-154d-47ec-92a2-88588306fbcc")]
        [FunctionBasedNewInstanceDefaultFieldValue("<f:function name=\"Composite.Utils.Guid.NewGuid\" xmlns:f=\"http://www.composite.net/ns/function/1.0\" />")]
        Guid Id { get; set; }

        [ImmutableFieldId("35ac3650-afe9-4acf-9e98-621b576312d4")]
        [StoreFieldType(PhysicalStoreFieldType.String, 128)]
        string Key { get; set; }

        [ImmutableFieldId("07330426-8fbf-40b2-9894-6c3a45abb172")]
        [StoreFieldType(PhysicalStoreFieldType.String, 512, IsNullable = true)]
        string ModelType { get; set; }

        [ImmutableFieldId("4081688c-a8e7-4339-9bcc-9e0a0cf643e8")]
        [StoreFieldType(PhysicalStoreFieldType.String, 256)]
        string To { get; set; }

        [ImmutableFieldId("a44cd1c4-9f77-4859-80c3-553696fca462")]
        [StoreFieldType(PhysicalStoreFieldType.String, 256, IsNullable = true)]
        string Bcc { get; set; }

        [ImmutableFieldId("f6315390-0595-4b20-8b89-d8459daa4707")]
        [StoreFieldType(PhysicalStoreFieldType.String, 256)]
        string From { get; set; }

        [ImmutableFieldId("c7a4ef88-3c73-4cca-89af-67455bf8e7d9")]
        [StoreFieldType(PhysicalStoreFieldType.String, 256)]
        string Subject { get; set; }

        [ImmutableFieldId("c52dda12-1ae2-4f32-bfc2-bd971c3cfb9b")]
        [StoreFieldType(PhysicalStoreFieldType.LargeString)]
        string Body { get; set; }
    }
}
