using System;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using Composite.Data;

using CompositeC1Contrib.Sorting;

namespace CompositeC1Contrib.FormBuilder.Data.Types
{
    public interface IFormField : IGenericSortable
    {
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("78eb1133-5386-47b7-8736-c189c3c946f8")]
        [ForeignKey(typeof(IForm), "Id", AllowCascadeDeletes = true)]
        Guid FormId { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 64)]
        [ImmutableFieldId("f809e2cf-ec9a-4e3e-8e04-b6793b028ae7")]
        string Type { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 128)]
        [ImmutableFieldId("04500a7a-de4c-450a-a62d-a649e5ec500d")]
        string Label { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.String, 512, IsNullable = true)]
        [ImmutableFieldId("28bb350f-352f-4440-9542-088c502b411a")]
        string DefaultValue { get; set; }

        [NotNullValidator]
        [StoreFieldType(PhysicalStoreFieldType.String, 256)]
        [ImmutableFieldId("a92c0dd2-6518-4630-b0c5-625339876e3f")]
        string HelpText { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.String, 512, IsNullable = true)]
        [ImmutableFieldId("00987c63-6a94-48fd-9703-a02368b9a001")]
        string ValidationRule { get; set; }
    }
}
