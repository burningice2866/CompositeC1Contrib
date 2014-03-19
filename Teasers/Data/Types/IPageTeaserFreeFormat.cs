using Composite.Data;

using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using CompositeC1Contrib.Teasers.C1Console.WorkFlows;

namespace CompositeC1Contrib.Teasers.Data.Types
{
    [ImmutableTypeId("809149AA-5A4A-4A0C-83BF-2310588FB5A6")]
    [Title("Page Teaser (Free Format)")]
    [Icon("editor-sourceview")]
    [EditWorkflow(typeof(EditPageTeaserFreeFormatWorkFlow))]
    public interface IPageTeaserFreeFormat : IPageTeaser
    {
        [ImmutableFieldId("FABC3247-BA58-4EBF-B957-30E803355A5A")]
        [StoreFieldType(PhysicalStoreFieldType.LargeString)]
        [NotNullValidator]
        string Content { get; set; }
    }
}
