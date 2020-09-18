using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(External.Metadata))]
    public partial class External
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.AssistedMovements))]
            [Id(4)]
            public ExternalBayManualParameters AssistedMovements { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ExtraRace))]
            [Unit("mm")]
            [Id(7)]
            public double ExtraRace { get; set; }

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
            public ExternalBayManualParameters ManualMovements { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Race))]
            [Unit("mm")]
            [Id(1)]
            public double Race { get; set; }

            #endregion
        }

        #endregion
    }
}
