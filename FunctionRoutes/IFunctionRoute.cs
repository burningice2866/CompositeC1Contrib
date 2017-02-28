using System;

using Composite.Data;
using Composite.Data.Hierarchy;
using Composite.Data.Hierarchy.DataAncestorProviders;

namespace CompositeC1Contrib.FunctionRoutes
{
    [DataScope("public")]
    [Title("Function route")]
    [LabelPropertyName("Function")]
    [AutoUpdateble]
    [DataAncestorProvider(typeof(NoAncestorDataAncestorProvider))]
    [ImmutableTypeId("e2e502b6-953f-4a87-a16d-3a85737dcd4c")]
    [KeyPropertyName("Id")]
    public interface IFunctionRoute : IData
    {
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ImmutableFieldId("80e77965-0d0c-456e-9715-2b2dfbcd9558")]
        Guid Id { get; set; }

        [StoreFieldType(PhysicalStoreFieldType.String, 256)]
        [ImmutableFieldId("69f5316b-a57c-4fec-8b69-6a2003305b02")]
        string Function { get; set; }
    }
}
