using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using Composite.Data;

namespace CompositeC1Contrib.Teasers.Data.Types
{
    [ImmutableTypeId("9951B026-7850-44C6-99AE-0053CB5138DA")]
    [Title("Shared Teaser Free Format")]
    public interface ISharedTeaserFreeFormat : ISharedTeaser
    {
        [ImmutableFieldId("FABC3247-BA58-4EBF-B957-30E803355A5A")]
        [StoreFieldType(PhysicalStoreFieldType.LargeString)]
        [NotNullValidator]
        string Content { get; set; }
    }
}
