using System;

using Composite.Data;
using Composite.Data.Hierarchy;

namespace CompositeC1Contrib.FormBuilder.Data.Types
{
    [AutoUpdateble]
    [KeyPropertyName("Id")]
    [DataScope(DataScopeIdentifier.PublicName)]
    [ImmutableTypeId("e290d40a-a142-4d0a-a999-20598a3256e8")]
    [Title("Formbuilder default field")]
    [LabelPropertyName("Label")]
    [DataAncestorProvider(typeof(FormDataAncestorProvider))]
    [RelevantToUserType(UserType.Developer)]
    public interface IDefaultFormField : IFormField
    {
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("23f53dc0-d309-4409-96e1-cf2915dda674")]
        [FunctionBasedNewInstanceDefaultFieldValue("<f:function name=\"Composite.Utils.Guid.NewGuid\" xmlns:f=\"http://www.composite.net/ns/function/1.0\" />")]
        Guid Id { get; set; }
    }
}
