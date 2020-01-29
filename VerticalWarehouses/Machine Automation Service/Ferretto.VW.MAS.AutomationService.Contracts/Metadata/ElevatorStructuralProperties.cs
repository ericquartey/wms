using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(ElevatorStructuralProperties.Metadata))]
    partial class ElevatorStructuralProperties
    {
        private class Metadata
        {
            [Id(1)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.BeltRigidity))]
            public int BeltRigidity { get; set; }

            [Id(2)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.BeltSpacing))]
            public double BeltSpacing { get; set; }

            [Id(3)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorWeight))]
            public double ElevatorWeight { get; set; }

            [Id(4)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.HalfShaftLength))]
            public double HalfShaftLength { get; set; }

            [Id(5)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.PulleyDiameter))]
            public double PulleyDiameter { get; set; }

            [Id(6)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShaftDiameter))]
            public double ShaftDiameter { get; set; }

            [Id(7)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShaftElasticity))]
            public double ShaftElasticity { get; set; }
        }
    }
}
