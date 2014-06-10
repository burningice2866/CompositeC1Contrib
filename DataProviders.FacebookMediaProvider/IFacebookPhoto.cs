using System;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace CompositeC1Contrib.DataProviders.FacebookMediaProvider
{
    [KeyPropertyName("Id")]
    [DataScope(DataScopeIdentifier.PublicName)]
    [Title("Facebook photo")]
    [LabelPropertyName("Title")]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    public interface IFacebookPhoto : IData
    {
        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 255)]
        [ImmutableFieldId("dfd59060-f01a-4444-98af-8e26481cfd43")]
        string Id { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 255)]
        [ImmutableFieldId("dfd59060-f01a-4444-98af-8e26481cfd43")]
        string AlbumId { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 255)]
        [ImmutableFieldId("686aa5fa-c118-4907-a37b-85de0dd211f6")]
        string Title { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.DateTime)]
        [ImmutableFieldId("eb53de32-ab9f-4036-bd93-c03a5aabffe8")]
        DateTime CreatedTime { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.DateTime)]
        [ImmutableFieldId("a3eb071b-a910-463c-947e-2d6b2c62fc3b")]
        DateTime UpdatedTime { get; set; }
    }
}
