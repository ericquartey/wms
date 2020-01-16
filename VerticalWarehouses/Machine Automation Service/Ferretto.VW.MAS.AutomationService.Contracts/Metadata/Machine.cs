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
            public System.Collections.Generic.IEnumerable<Bay> Bays { get; set; }

            [Category(Category = nameof(Vertimag.Elevator), ResourceType = typeof(Vertimag))]
            public Elevator Elevator { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_Height))]
            public double Height { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadUnitMaxHeight))]
            public double LoadUnitMaxHeight { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadUnitMaxNetWeight))]
            [Unit("kg")]
            public double LoadUnitMaxNetWeight { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadUnitTare))]
            [Unit("kg")]
            public double LoadUnitTare { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_MaxGrossWeight))]
            [Unit("kg")]
            public double MaxGrossWeight { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_ModelName))]
            public string ModelName { get; set; }

            [ScaffoldColumn(false)]
            public System.Collections.Generic.IEnumerable<CellPanel> Panels { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_SerialNumber))]
            public string SerialNumber { get; set; }
        }
    }
}
