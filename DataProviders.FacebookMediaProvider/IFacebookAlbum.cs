using System;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace CompositeC1Contrib.DataProviders.FacebookMediaProvider
{
    [AutoUpdateble]
    [KeyPropertyName("Id")]
    [DataScope(DataScopeIdentifier.PublicName)]
    [ImmutableTypeId("4812fa41-99e1-414e-81e1-c39fafa8ee4c")]
    [Title("Facebook album")]
    [LabelPropertyName("Title")]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    public interface IFacebookAlbum : IData
    {
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("d1dada52-905e-480f-8fc3-d0b8e915cd67")]
        [FunctionBasedNewInstanceDefaultFieldValue("<f:function name=\"Composite.Utils.Guid.NewGuid\" xmlns:f=\"http://www.composite.net/ns/function/1.0\" />")]
        Guid Id { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 255)]
        [ImmutableFieldId("3702bf31-69b1-4532-bec9-31cc08769aff")]
        string AlbumId { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 255)]
        [ImmutableFieldId("4b15cfe8-f7da-47d1-8a39-5ee573985a44")]
        string Title { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 255)]
        [ImmutableFieldId("b4469843-0394-4afd-897f-1bb4de31fd50")]
        string AccessToken { get; set; }
    }

}
