using System;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using Composite.Data;
using Composite.Data.Hierarchy;

namespace CompositeC1Contrib.Email.Data.Types
{
    [AutoUpdateble]
    [KeyPropertyName("Id")]
    [DataScope(DataScopeIdentifier.PublicName)]
    [ImmutableTypeId("b37fa3e8-95c0-45fe-9957-c8af313234ef")]
    [Title("Mail message")]
    [LabelPropertyName("Subject")]
    [DataAncestorProvider(typeof(MailMessageDataAncestorProvider))]
    public interface IEmailMessage : IChangeHistory, IData
    {
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("36c0eef1-fc09-4ef1-822e-8bffbc28e1b8")]
        [FunctionBasedNewInstanceDefaultFieldValue("<f:function name=\"Composite.Utils.Guid.NewGuid\" xmlns:f=\"http://www.composite.net/ns/function/1.0\" />")]
        Guid Id { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("777e7874-fe49-4c31-a568-be1d1e204055")]
        [ForeignKey(typeof(IEmailQueue), "Id", AllowCascadeDeletes = true)]
        Guid QueueId { get; set; }

        [NotNullValidator]
        [ImmutableFieldId("8169a21f-9d9c-4570-83a1-27bf59d16edb")]
        [StoreFieldType(PhysicalStoreFieldType.String, 512)]
        string Subject { get; set; }

        [NotNullValidator]
        [ImmutableFieldId("6a7e3f0b-c93e-4eb5-82d4-d4612e5f7e4d")]
        [StoreFieldType(PhysicalStoreFieldType.LargeString)]
        string SerializedMessage { get; set; }
    }
}
