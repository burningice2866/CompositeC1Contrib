using System;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace CompositeC1Contrib.Web.Security
{
    [AutoUpdateble]
    [KeyPropertyName("Id")]
    [DataScope(DataScopeIdentifier.PublicName)]
    [ImmutableTypeId("{06C64621-2319-422B-A75B-364C530063FA}")]
    [Caching(CachingType.Full)]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    public interface IMembershipUser : IData
    {
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("{AC79B7BF-2FC2-48A6-BC83-A27E1AF62362}")]
        Guid Id { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.LargeString)]
        [ImmutableFieldId("{768CE0CD-9D38-4FC3-B3CF-838444475E9F}")]
        string Comment { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.DateTime)]
        [ImmutableFieldId("{76A488FF-A137-4FDE-B367-F7E5C9BDFB2C}")]
        DateTime CreationDate { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.DateTime)]
        [ImmutableFieldId("{1A56F1FA-8B35-45AA-8A63-B42CE2E12680}")]
        DateTime LastLoginDate { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.DateTime)]
        [ImmutableFieldId("{AB70DCA9-5972-43E2-A096-5C67BEFD3124}")]
        DateTime LastActivityDate { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.DateTime)]
        [ImmutableFieldId("{43B6FC71-46BA-4780-B8C7-83718E68524B}")]
        DateTime LastLockoutDate { get; set; }
    }
}
