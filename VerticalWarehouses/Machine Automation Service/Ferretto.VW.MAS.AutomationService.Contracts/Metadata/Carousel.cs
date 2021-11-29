using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Carousel.Metadata))]
    public partial class Carousel
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.AssistedMovements))]
            [Id(4)]
            public CarouselManualParameters AssistedMovements { get; set; }

            [Range(6, 15, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Range))]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Carousel_BayFindZeroLimit))]
            [Unit("mm")]
            [Id(8)]
            public int BayFindZeroLimit { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Carousel_ElevatorDistance))]
            [Unit("mm")]
            [Id(1)]
            public double ElevatorDistance { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Carousel_HomingCreepSpeed))]
            [Unit("mm/s")]
            [Id(2)]
            public double HomingCreepSpeed { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Carousel_HomingFastSpeed))]
            [Unit("mm/s")]
            [Id(3)]
            public double HomingFastSpeed { get; set; }

            [ScaffoldColumn(false)]
            public double LastIdealPosition { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.ManualMovements))]
            [Id(6)]
            public CarouselManualParameters ManualMovements { get; set; }

            #endregion
        }

        #endregion
    }
}
