using Ferretto.VW.MAS.Scaffolding.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(ElevatorAxisManualParameters.Metadata))]
    partial class ElevatorAxisManualParameters
    {
        class Metadata {

            [Id(1)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.FeedRate))]
            public double FeedRate { get; set; }

            [Id(2)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.FeedRateAfterZero))]
            public double FeedRateAfterZero { get; set; }

            [Id(3)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.TargetDistance))]
            public double TargetDistance { get; set; }

            [Id(4)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.TargetDistanceAfterZero))]
            public double TargetDistanceAfterZero { get; set; }

        }
    }
}
