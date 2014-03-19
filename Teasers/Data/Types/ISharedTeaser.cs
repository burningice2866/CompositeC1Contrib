using System;

using Composite.Data;

namespace CompositeC1Contrib.Teasers.Data.Types
{
    [KeyPropertyName("Id")]
    [LabelPropertyName("Name")]
    public interface ISharedTeaser : ITeaser
    {
        [ImmutableFieldId("2706E8BB-9BAA-46FA-9AC8-5B5CE7CE6A3E")]
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [FunctionBasedNewInstanceDefaultFieldValue("<f:function name=\"Composite.Utils.Guid.NewGuid\" xmlns:f=\"http://www.composite.net/ns/function/1.0\" />")]
        Guid Id { get; set; }

        [ImmutableFieldId("8F47BD4D-BA20-457C-B270-B317BBB9C5D0")]
        [StoreFieldType(PhysicalStoreFieldType.Guid)]
        [ForeignKey(typeof(ISharedTeaserGroup), "Id", AllowCascadeDeletes = true)]
        [GroupByPriority(1)]
        Guid TeaserGroup { get; set; }

        [ImmutableFieldId("7f616aeb-4df0-4c2e-82b8-4c1c2aa705b6")]
        [StoreFieldType(PhysicalStoreFieldType.Boolean)]
        bool InclInRotation { get; set; }
    }
}
