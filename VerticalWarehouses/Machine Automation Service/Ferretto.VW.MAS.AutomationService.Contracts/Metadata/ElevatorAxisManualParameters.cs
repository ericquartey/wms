using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(ElevatorAxisManualParameters.Metadata))]
    partial class ElevatorAxisManualParameters
    {
        class Metadata
        {

            [Id(1)]
            [Editable(true)]
            [Range(0D, 1D, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Range))]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.FeedRate))]
            public double FeedRate { get; set; }

            [Id(2)]
            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.FeedRateAfterZero))]
            [Range(0D, 1D, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Range))]
            public double FeedRateAfterZero { get; set; }

            [Id(3)]
            [Editable(true)]
            [Unit("mm")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.TargetDistance))]
            public double TargetDistance { get; set; }

            [Id(4)]
            [Editable(true)]
            [Unit("mm")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.TargetDistanceAfterZero))]
            public double TargetDistanceAfterZero { get; set; }

        }
    }
}
