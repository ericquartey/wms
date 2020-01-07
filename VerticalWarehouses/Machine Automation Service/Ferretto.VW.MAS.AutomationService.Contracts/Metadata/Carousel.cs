using Ferretto.VW.MAS.Scaffolding.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Carousel.Metadata))]
    public partial class Carousel
    {
        class Metadata
        {
            [Unfold]
            public CarouselManualParameters AssistedMovements { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Carousel_ElevatorDistance))]
            [Unit("mm")]
            public double ElevatorDistance { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Carousel_HomingCreepSpeed))]
            [Unit("mm/s")]
            public double HomingCreepSpeed { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Carousel_HomingFastSpeed))]
            [Unit("mm/s")]
            public double HomingFastSpeed { get; set; }

            [ScaffoldColumn(false)]
            public bool IsHomingExecuted { get; set; }

            [ScaffoldColumn(false)]
            public double LastIdealPosition { get; set; }

            [Unfold]
            public CarouselManualParameters ManualMovements { get; set; }
        }
    }
}
