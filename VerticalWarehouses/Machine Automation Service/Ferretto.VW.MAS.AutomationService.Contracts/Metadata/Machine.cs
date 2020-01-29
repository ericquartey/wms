using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Machine.Metadata))]
    public partial class Machine
    {
        private class Metadata
        {
            [Category(Category = nameof(Vertimag.Bay), ResourceType = typeof(Vertimag))]
            [CategoryParameter(nameof(Bay.Number), ValueStringifierType = typeof(EnumValueStringifier))]
            [Id(100)]
            [Offset(100)]
            [PullToRoot, Unfold]
            public System.Collections.Generic.IEnumerable<Bay> Bays { get; set; }

            [Category(Category = nameof(Vertimag.Elevator), ResourceType = typeof(Vertimag))]
            [Offset(500)]
            [PullToRoot, Unfold]
            public Elevator Elevator { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_Height))]
            [Id(3)]
            public double Height { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadUnitMaxHeight))]
            [Id(4)]
            public double LoadUnitMaxHeight { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadUnitMaxNetWeight))]
            [Unit("kg")]
            [Id(5)]
            public double LoadUnitMaxNetWeight { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadUnitTare))]
            [Unit("kg")]
            [Id(6)]
            public double LoadUnitTare { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_MaxGrossWeight))]
            [Unit("kg")]
            [Id(7)]
            public double MaxGrossWeight { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_ModelName))]
            [Id(1)]
            public string ModelName { get; set; }

            [ScaffoldColumn(false)]
            public System.Collections.Generic.IEnumerable<CellPanel> Panels { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_SerialNumber))]
            [Id(2)]
            public string SerialNumber { get; set; }
        }
    }
}
